using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonEntityModel.EntityModel
{
    public class ProductPurchaseMaster
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int PurchaseMasterId { get; set; }

        [Column(TypeName = "nvarchar(30)")]
        public string PurchaseCode { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(50)")]
        public string RestaurantCode { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string Comment { get; set; }

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
    }
}
