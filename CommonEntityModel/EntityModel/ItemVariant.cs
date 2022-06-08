using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonEntityModel.EntityModel
{
    public class ItemVariant
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ItemVariantId { get; set; }

        public int ItemId { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string VariantName { get; set; }

        public int VariantId { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal VariantWiseItemprice { get; set; }

        public int VariantMasterId { get; set; }

        //public VariantMaster VariantMaster { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(50)")]
        public string RestaurantCode { get; set; }

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
