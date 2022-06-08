using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Database.ModelClass
{
    public class RestaurantUserInfoData
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string NormalizedEmail { get; set; }
        public string UserStatus { get; set; }
        public string Password { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string VerificationCode { get; set; }
    }
}
