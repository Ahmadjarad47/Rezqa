using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezqa.Domain.Entities
{
    public class AppUsers : IdentityUser<Guid>
    {
        public string image { get; set; }
        public ICollection<Ad> ads { get; set; } = new List<Ad>();
    }
}
