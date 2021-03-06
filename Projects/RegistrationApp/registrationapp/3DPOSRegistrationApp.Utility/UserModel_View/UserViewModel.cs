using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Utility.UserModel_View
{
    public class UserViewModel
    {
        public List<UserModel> Users { get; set; }

        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int UserRoleID { get; set; }
        public string UserRole { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UserStatus { get; set; }
    }
}
