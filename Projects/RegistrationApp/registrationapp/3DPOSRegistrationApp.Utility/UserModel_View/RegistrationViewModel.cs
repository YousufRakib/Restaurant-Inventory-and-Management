using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _3DPOSRegistrationApp.Utility.UserStatus;

namespace _3DPOSRegistrationApp.Utility.UserModel_View
{
    public class RegistrationViewModel
    {
        public int id { get; set; }

        [Required]
        public string FullName { get; set; }
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [Remote(action: "IsEmailInUse", controller: "User")]
        //[DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string LastPassword { get; set; }
        public int UserRoleID { get; set; }
        public string UserRole { get; set; }
        public int CretedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserStatusId { get; set; }
    }
}
