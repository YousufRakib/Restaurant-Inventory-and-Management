using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace _3DPOSRegistrationApp.Database.Model
{
    public class AspNetUsers : IdentityUser<int>
    {
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
    }
}
