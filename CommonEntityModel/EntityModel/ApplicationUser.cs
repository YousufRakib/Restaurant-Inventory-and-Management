using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonEntityModel.EntityModel
{
    public class ApplicationUser : IdentityUser<int>
    {
        //[PersonalData]
        //[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        //public int UserID { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string FullName { get; set; }

        [Column(TypeName = "int")]
        public int CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "int")]
        public int UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }

        [Column(TypeName = "int")]
        public int UserStatus { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string RestaurantCode { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string DefaultPassword { get; set; }
    }
}
