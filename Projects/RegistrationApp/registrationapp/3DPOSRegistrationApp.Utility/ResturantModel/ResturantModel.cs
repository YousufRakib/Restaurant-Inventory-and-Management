using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Utility.ResturantModel
{
    public class ResturantModel
    {
        public int ResturantID { get; set; }

        public string ResturentCode { get; set; }

        public string RestaurantName { get; set; }

        public string CompanyRegistrationNo { get; set; }

        public string ContactPersion { get; set; }

        public string ContactEmail { get; set; }

        public string ContactPhone { get; set; }

        public string Country { get; set; }

        public string District { get; set; }

        public string PostCode { get; set; }

        public string Address { get; set; }

        public bool IsDBCreated { get; set; }

        public bool IsTableCreated { get; set; }

        public string DBCreationStatus { get; set; }

        public string DatabaseName { get; set; }

        public int DBCatalogID { get; set; }

        public string DBConnectionString { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int UserStatus { get; set; }

        public string RestaurantStatus { get; set; }

        public string RestaurantUserId { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
