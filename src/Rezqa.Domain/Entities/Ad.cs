using Rezqa.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rezqa.Domain.Entities
{
    public class Ad : BaseEntity
    {

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string[]? ImageUrl { get; set; }

        public string location { get; set; }
        public bool isActive { get; set; } = false;


        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int SubCategoryId { get; set; }

        public SubCategory SubCategory { get; set; }


        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public AppUsers AppUsers { get; set; }
        public ICollection<AdFieldValue> FieldValues { get; set; } = new List<AdFieldValue>();


    }
}
