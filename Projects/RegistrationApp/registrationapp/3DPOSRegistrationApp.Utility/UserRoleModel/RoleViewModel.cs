using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Utility.UserRoleModel
{
    public class RoleViewModel
    {
        [Required]
        [Remote(action: "IsRoleInUse", controller: "Administration")]
        public string RoleName { get; set; }
    }
}
