using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezqa.Application.Interfaces
{
    public interface ISubCategoryRepository : IRepository<Rezqa.Domain.Entities.SubCategory>
    {
        Task<bool> ExistsByTitleAsync(string title);
        Task<bool> ExistsByCategoryIdAsync(int categoryId);
        Task<bool> CategoryExistsAsync(int categoryId);
    }
}
