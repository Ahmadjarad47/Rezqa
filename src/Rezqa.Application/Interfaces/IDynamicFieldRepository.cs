using Rezqa.Domain.Entities;

namespace Rezqa.Application.Interfaces;

public interface IDynamicFieldRepository : IRepository<DynamicField>
{

    Task<bool> ExistsByNameAsync(string name, int categoryId, int? subCategoryId = null);
    Task<List<DynamicField>> GetByCategoryAsync(int categoryId, int? subCategoryId = null);
    Task<List<DynamicField>> GetByTypeAsync(string type);
}