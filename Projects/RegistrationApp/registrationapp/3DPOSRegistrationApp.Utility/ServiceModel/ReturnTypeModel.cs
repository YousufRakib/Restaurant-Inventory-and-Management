using _3DPOSRegistrationApp.Utility.ResturantModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Utility.ServiceModel
{
    public class ReturnTypeModel
    {
        public string True = "True";
        public string False = "False";
        public string Exception = "Exception";
        public List<RestaurantUsers> restaurantUsersList { get; set; }
    }
}
