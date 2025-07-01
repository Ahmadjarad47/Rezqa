using Rezqa.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezqa.Domain.Entities;

public class SubCategory : BaseEntity
{

    public string Title { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
  
    public ICollection<DynamicField> DynamicFields { get; set; } = new List<DynamicField>();

}
