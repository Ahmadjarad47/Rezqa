using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;

namespace Rezqa.Infrastructure.Persistence.Repositories;

public class SubCategoryRepository : ISubCategoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private const string AllSubCategoriesCacheKey = "AllSubCategories";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public SubCategoryRepository(ApplicationDbContext context, IMemoryCache memoryCache)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<SubCategory?> GetByIdAsync(int id)
    {
        string cacheKey = $"SubCategory_{id}";

        if (!_memoryCache.TryGetValue(cacheKey, out SubCategory? subCategory))
        {
            subCategory = await _context.SubCategories
                .AsNoTracking()
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc => sc.Id == id && !sc.IsDeleted)
                .ConfigureAwait(false);

            if (subCategory != null)
            {
                _memoryCache.Set(cacheKey, subCategory, CacheDuration);
            }
        }

        return subCategory;
    }

    public async Task<IEnumerable<SubCategory>> GetAllAsync()
    {
        if (!_memoryCache.TryGetValue(AllSubCategoriesCacheKey, out IEnumerable<SubCategory>? subCategories))
        {
            subCategories = await _context.SubCategories
                .AsNoTracking()
                .Include(sc => sc.Category)
                .Where(sc => !sc.IsDeleted)
                .ToListAsync()
                .ConfigureAwait(false);

            _memoryCache.Set(AllSubCategoriesCacheKey, subCategories, CacheDuration);
        }

        return subCategories ?? Enumerable.Empty<SubCategory>();
    }

    public async Task<SubCategory> AddAsync(SubCategory entity)
    {
        await _context.SubCategories.AddAsync(entity).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        InvalidateCaches(entity);
        return entity;
    }

    public async Task UpdateAsync(SubCategory entity)
    {
        _context.SubCategories.Update(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        InvalidateCaches(entity);
    }

    public async Task DeleteAsync(SubCategory entity)
    {
        entity.IsDeleted = true;
        await _context.SaveChangesAsync().ConfigureAwait(false);

        InvalidateCaches(entity);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        string cacheKey = $"SubCategory_Exists_{id}";

        if (!_memoryCache.TryGetValue(cacheKey, out bool exists))
        {
            exists = await _context.SubCategories
                .AsNoTracking()
                .AnyAsync(sc => sc.Id == id && !sc.IsDeleted)
                .ConfigureAwait(false);

            _memoryCache.Set(cacheKey, exists, CacheDuration);
        }

        return exists;
    }

    public async Task<bool> ExistsByTitleAsync(string title)
    {
        string cacheKey = $"SubCategory_Exists_Title_{title}";

        if (!_memoryCache.TryGetValue(cacheKey, out bool exists))
        {
            exists = await _context.SubCategories
                .AsNoTracking()
                .AnyAsync(sc => sc.Title == title && !sc.IsDeleted)
                .ConfigureAwait(false);

            _memoryCache.Set(cacheKey, exists, CacheDuration);
        }

        return exists;
    }

    public async Task<bool> ExistsByCategoryIdAsync(int categoryId)
    {
        string cacheKey = $"SubCategory_Exists_Category_{categoryId}";

        if (!_memoryCache.TryGetValue(cacheKey, out bool exists))
        {
            exists = await _context.SubCategories
                .AsNoTracking()
                .AnyAsync(sc => sc.CategoryId == categoryId && !sc.IsDeleted)
                .ConfigureAwait(false);

            _memoryCache.Set(cacheKey, exists, CacheDuration);
        }

        return exists;
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        string cacheKey = $"Category_Exists_{categoryId}";

        if (!_memoryCache.TryGetValue(cacheKey, out bool exists))
        {
            exists = await _context.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Id == categoryId && !c.IsDeleted)
                .ConfigureAwait(false);

            _memoryCache.Set(cacheKey, exists, CacheDuration);
        }

        return exists;
    }

    private void InvalidateCaches(SubCategory entity)
    {
        // Invalidate all relevant cache entries
        _memoryCache.Remove(AllSubCategoriesCacheKey);
        _memoryCache.Remove($"SubCategory_{entity.Id}");
        _memoryCache.Remove($"SubCategory_Exists_{entity.Id}");
        _memoryCache.Remove($"SubCategory_Exists_Title_{entity.Title}");
        _memoryCache.Remove($"SubCategory_Exists_Category_{entity.CategoryId}");

        // Also invalidate parent category cache if needed
        _memoryCache.Remove($"Category_{entity.CategoryId}");
    }
}