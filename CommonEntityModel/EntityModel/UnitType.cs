﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonEntityModel.EntityModel
{
    public class UnitType
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int UnitTypeId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(50)")]
        public string UnitTypeName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(50)")]
        public string RestaurantCode { get; set; }

        //[Index(IsUnique = true)]
        //public int RestaurantID { get; set; }

        [Column(TypeName = "int")]
        public int CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "int")]
        public int UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
        //public ICollection<Product> Products { get; set; }
    }
}
