using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;


namespace Rezqa.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private const string AllCategoriesCacheKey = "AllCategories";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(8);

    public CategoryRepository(ApplicationDbContext context, IMemoryCache memoryCache)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        string cacheKey = $"Category_{id}";

        if (!_memoryCache.TryGetValue(cacheKey, out Category? category))
        {
            category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted)
                .ConfigureAwait(false);

            if (category != null)
            {
                _memoryCache.Set(cacheKey, category, CacheDuration);
            }
        }

        return category;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        if (!_memoryCache.TryGetValue(AllCategoriesCacheKey, out IEnumerable<Category>? categories))
        {
            categories = await _context.Categories
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .ToListAsync()
                .ConfigureAwait(false);

            _memoryCache.Set(AllCategoriesCacheKey, categories, CacheDuration);
        }

        return categories ?? Enumerable.Empty<Category>();
    }

    public async Task<Category> AddAsync(Category entity)
    {
        await _context.Categories.AddAsync(entity).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        // Invalidate the "all categories" cache since we added a new one
        _memoryCache.Remove(AllCategoriesCacheKey);

        return entity;
    }

    public async Task UpdateAsync(Category entity)
    {
        _context.Categories.Update(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        // Invalidate both the individual category and "all categories" cache
        _memoryCache.Remove($"Category_{entity.Id}");
        _memoryCache.Remove(AllCategoriesCacheKey);
    }

    public async Task DeleteAsync(Category entity)
    {
        entity.IsDeleted = true;
        await _context.SaveChangesAsync().ConfigureAwait(false);

        // Invalidate caches
        _memoryCache.Remove($"Category_{entity.Id}");
        _memoryCache.Remove(AllCategoriesCacheKey);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Id == id && !c.IsDeleted)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsByTitleAsync(string title)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Title == title && !c.IsDeleted)
            .ConfigureAwait(false);
    }
}