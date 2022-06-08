using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.ServiceModel
{
    public class ResponseMessage
    {
        public string EmailExist = "This user/email already exists!";
        public string CountryNameExist = "This country name already exists!";
        public string RoleExist = "This role already exists!";
        public string WrongLoginInfo = "Invalid Email or Password!";
        public string TerminatedUser = "This user is terminated from the system!";
        public string WrongRoleInfo = "Invalid Role Attempt!";
        public string RoleNotExist = "UserRole not found!";
        public string UserNotExist = "User not found!";
        public string EmptyErrorDetails = "No error found!";
        public string RestaurantDBNotAdded = "Restaurant database don't added!";
        public string RestaurantTableNotAdded = "Restaurant table don't added!";
        public string RestaurantUserNotAdded = "Restaurant user don't added in database!";
        public string RestaurantUserNotAddedInRestaurantDatabase = "Restaurant user didn't add in restaurant database!";
        public string RestaurantInformationNotUpdated = "Restaurantinformation didn't update!";
        public string TryCatchError = "An error occured.To solve the error, Please check Error Log or Contact with your support engineer";
        public string EmptyRestaurantUser = "No restaurant user added yet!";
        public string DBCatalogDoNotRemove = "This DBCatalog can't remove!";
        public string DBCatalogExistOrTryCatchError = "This database already exist or Please check Error Log or Contact with your support engineer";
    }
}
