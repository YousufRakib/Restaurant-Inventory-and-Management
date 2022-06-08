using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Utility.CommonModel
{
    public class DistrictModel
    {
        public int DistrictID { get; set; }

        [Required]
        [Remote(action: "IsDistrictInUse", controller: "Common")]
        public string DistrictName { get; set; }
        public int CountryID { get; set; }
        public string CountryName { get; set; }
    }
}
