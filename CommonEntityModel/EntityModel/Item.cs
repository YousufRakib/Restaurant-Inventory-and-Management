﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonEntityModel.EntityModel
{
    public class Item
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ItemId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(50)")]
        public string ItemName { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string ItemDescription { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string ItemImagePath { get; set; }

        public int VariantMasterId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(50)")]
        public string RestaurantCode { get; set; }

        [Column(TypeName = "bit")]
        public bool Status { get; set; }

        [Column(TypeName = "bit")]
        public bool IsDeleted { get; set; }

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
