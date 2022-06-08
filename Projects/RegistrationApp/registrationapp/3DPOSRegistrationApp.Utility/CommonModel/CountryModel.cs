using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Utility.CommonModel
{
    public class CountryModel
    {
        public int CountryID { get; set; }

        [Required]
        [Remote(action: "IsCountryInUse", controller: "Common")]
        public string CountryName { get; set; }
    }
}
