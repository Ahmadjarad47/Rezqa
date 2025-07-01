using Microsoft.EntityFrameworkCore;
using Rezqa.Application.Common.Models;
using Rezqa.Application.Features.Ad.Request.Query;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

namespace Rezqa.Infrastructure.Persistence.Repositories
{
    public class AdRepository : IRepository<Ad>, IAdRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string AllAdsCacheKey = "AllAdsCacheKey";

        public AdRepository(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<PaginatedResult<Ad>> SearchAdsAsync(SearchAdsQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 || request.PageSize > 100 ? 10 : request.PageSize;

            IQueryable<Ad> query = _context.ads
                .AsNoTracking()
                .Include(c => c.Category)
                .Include(a => a.AppUsers)
                .Include(a => a.FieldValues).ThenInclude(f => f.DynamicField);

            if (request.CategoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == request.CategoryId.Value);
            }

            if (request.SubCategoryId.HasValue)
            {
                query = query.Where(a => a.SubCategoryId == request.SubCategoryId.Value);
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(a => a.Price >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(a => a.Price <= request.MaxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Location))
            {
                query = query.Where(a => a.location.Contains(request.Location));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var keywords = request.SearchTerm
                    .Trim()
                    .ToLower()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var keyword in keywords)
                {
                    var lowerKeyword = $"%{keyword}%";
                    query = query.Where(a =>
                        EF.Functions.Like(a.Title.ToLower(), lowerKeyword) ||
                        EF.Functions.Like(a.location.ToLower(), lowerKeyword) ||
                        EF.Functions.Like(a.Description.ToLower(), lowerKeyword)
                    );
                }
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<Ad>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<Ad> AddAsync(Ad entity)
        {
            await _context.ads.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Ad entity)
        {
            _context.ads.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ads.AnyAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Ad>> GetAdsByLocationAsync(string location)
        {
            return await _context.ads
                .AsNoTracking()
                .Where(a => a.location.Contains(location))
                .ToListAsync();
        }

        public async Task<IEnumerable<Ad>> GetAdsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _context.ads
                .AsNoTracking()
                .Where(a => a.Price >= minPrice && a.Price <= maxPrice)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ad>> GetAdsByUserIdAsync(Guid userId)
        {
            return await _context.ads
                .AsNoTracking()
                .Include(c => c.Category)
                .Include(a => a.AppUsers)
                .Include(a => a.FieldValues)
                .Where(a => a.AppUsers.Id == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ad>> GetAdsWithFieldValuesAsync()
        {
            return await _context.ads
                .AsNoTracking()
                .Include(a => a.AppUsers)
                .Include(a => a.FieldValues)
                .ToListAsync();
        }

        public async Task<Ad?> GetAdWithFieldValuesByIdAsync(int id)
        {
            return await _context.ads
                .AsNoTracking()
                .Include(a => a.AppUsers)
                .Include(a => a.FieldValues)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Ad>> GetAllAsync()
        {
            if (!_cache.TryGetValue(AllAdsCacheKey, out IEnumerable<Ad> ads))
            {
                ads = await _context.ads
                    .AsNoTracking()
                    .Include(app => app.AppUsers)
                    .Include(a => a.FieldValues)
                    .ToListAsync();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));
                _cache.Set(AllAdsCacheKey, ads, cacheEntryOptions);
            }
            return ads;
        }

        public async Task<Ad?> GetByIdAsync(int id)
        {
            return await _context.ads
                .AsNoTracking().Include(c => c.Category)
                .Include(a => a.AppUsers)
                .Include(a => a.FieldValues).ThenInclude(c => c.DynamicField)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateAsync(Ad entity)
        {
            _context.ads.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
