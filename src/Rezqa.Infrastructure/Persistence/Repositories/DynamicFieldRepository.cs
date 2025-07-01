using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;

namespace Rezqa.Infrastructure.Persistence.Repositories;

public class DynamicFieldRepository : IDynamicFieldRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private const string AllFieldsCacheKey = "AllDynamicFields";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public DynamicFieldRepository(ApplicationDbContext context, IMemoryCache memoryCache)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        int categoryId,
        int? subCategoryId = null
    )
    {
        string cacheKey = $"DynamicField_Exists_{name}_{categoryId}_{subCategoryId}";

        if (!_memoryCache.TryGetValue(cacheKey, out bool exists))
        {
            var query = _context.DynamicFields
                .AsNoTracking()
                .Where(x => x.Name == name && x.CategoryId == categoryId);

            query = subCategoryId.HasValue
                ? query.Where(x => x.SubCategoryId == subCategoryId)
                : query.Where(x => x.SubCategoryId == null);

            exists = await query.AnyAsync().ConfigureAwait(false);
            _memoryCache.Set(cacheKey, exists, CacheDuration);
        }

        return exists;
    }

    public async Task<List<DynamicField>> GetByCategoryAsync(
        int categoryId,
        int? subCategoryId = null
    )
    {
        string cacheKey = $"DynamicFields_Category_{categoryId}_SubCategory_{subCategoryId}";

        if (!_memoryCache.TryGetValue(cacheKey, out List<DynamicField> fields))
        {
            var query = _context
                .DynamicFields.AsNoTracking()
                .Include(m => m.Category)
                .Include(m => m.Options)
                .Where(x => x.CategoryId == categoryId);

            query = subCategoryId.HasValue
                ? query.Where(x => x.SubCategoryId == subCategoryId)
                : query.Where(x => x.SubCategoryId == null);

            fields = await query.ToListAsync().ConfigureAwait(false);
            _memoryCache.Set(cacheKey, fields, CacheDuration);
        }

        return fields;
    }

    public async Task<List<DynamicField>> GetByTypeAsync(string type)
    {
        string cacheKey = $"DynamicFields_Type_{type}";

        if (!_memoryCache.TryGetValue(cacheKey, out List<DynamicField> fields))
        {
            fields = await _context
                .DynamicFields
                .AsNoTracking()
                .Where(x => x.Type == type)
                .ToListAsync()
                .ConfigureAwait(false);
            _memoryCache.Set(cacheKey, fields, CacheDuration);
        }

        return fields;
    }

    public async Task<IEnumerable<DynamicField>> GetAllAsync()
    {
        if (!_memoryCache.TryGetValue(AllFieldsCacheKey, out IEnumerable<DynamicField> fields))
        {
            fields = await _context
                .DynamicFields
                .AsNoTracking()
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .Include(m => m.Options)
                .ToListAsync()
                .ConfigureAwait(false);
            _memoryCache.Set(AllFieldsCacheKey, fields, CacheDuration);
        }

        return fields;
    }

    public async Task<DynamicField> GetByIdAsync(int id)
    {
        string cacheKey = $"DynamicField_{id}";

        if (!_memoryCache.TryGetValue(cacheKey, out DynamicField field))
        {
            field = await _context
                .DynamicFields
                .AsNoTracking()
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .FirstOrDefaultAsync(d => d.Id == id)
                .ConfigureAwait(false);

            if (field != null)
            {
                _memoryCache.Set(cacheKey, field, CacheDuration);
            }
        }

        return field;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        string cacheKey = $"DynamicField_Exists_{id}";

        if (!_memoryCache.TryGetValue(cacheKey, out bool exists))
        {
            exists = await _context.DynamicFields
                .AsNoTracking()
                .AnyAsync(e => e.Id == id)
                .ConfigureAwait(false);
            _memoryCache.Set(cacheKey, exists, CacheDuration);
        }

        return exists;
    }

    public async Task<DynamicField> AddAsync(DynamicField entity)
    {
        await _context.DynamicFields.AddAsync(entity).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        InvalidateRelatedCaches(entity);
        return entity;
    }

    public async Task UpdateAsync(DynamicField entity)
    {
        _context.DynamicFields.Update(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        InvalidateRelatedCaches(entity);
    }

    public async Task DeleteAsync(DynamicField entity)
    {
        _context.DynamicFields.Remove(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        InvalidateRelatedCaches(entity);
    }

    private void InvalidateRelatedCaches(DynamicField entity)
    {
        // Invalidate all relevant cache entries
        _memoryCache.Remove(AllFieldsCacheKey);
        _memoryCache.Remove($"DynamicField_{entity.Id}");
        _memoryCache.Remove($"DynamicField_Exists_{entity.Id}");
        _memoryCache.Remove(
            $"DynamicFields_Category_{entity.CategoryId}_SubCategory_{entity.SubCategoryId}"
        );
        _memoryCache.Remove($"DynamicFields_Type_{entity.Type}");
    }
}
