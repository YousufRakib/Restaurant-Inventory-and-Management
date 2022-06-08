using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonEntityModel.EntityModel
{
    public class Product
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductId { get; set; }

        [Required(AllowEmptyStrings = false)] // or false
        [Column(TypeName = "nvarchar(50)")]
        public string ProductName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(50)")]
        public string RestaurantCode { get; set; }

        //[Index(IsUnique = true)]
        //public int RestaurantID { get; set; }

        [Column(TypeName = "int")]
        public int ProductCategoryId { get; set; }
        public ProductCategory ProductCategory { get; set; }

        [Column(TypeName = "int")]
        public int UnitTypeId { get; set; }
        public UnitType UnitType { get; set; }

        //[Column(TypeName = "decimal(18,5)")]
        //public decimal Price { get; set; }

        //public int Quantity { get; set; }

        //[Column(TypeName = "decimal(18,5)")]
        //public decimal Total { get; set; }

        //[Column(TypeName = "nvarchar(50)")]
        //public string Comment { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string CreatedByName { get; set; }

        [Column(TypeName = "int")]
        public int CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string UpdatedByName { get; set; }

        [Column(TypeName = "int")]
        public int UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
        //public ICollection<InventoryModel> InventoryModels { get; set; }
    }
}
