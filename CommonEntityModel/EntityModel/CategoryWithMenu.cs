﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonEntityModel.EntityModel
{
    public class CategoryWithMenu
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public int MenuId { get; set; }
        //public Menu Menu { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string CreatedByName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(50)")]
        public string RestaurantCode { get; set; }

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
