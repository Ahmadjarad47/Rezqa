using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezqa.Application.Interfaces
{
    public interface ICategoryRepository : IRepository<Rezqa.Domain.Entities.Category>
    {
        Task<bool> ExistsByTitleAsync(string title);
    }
}
