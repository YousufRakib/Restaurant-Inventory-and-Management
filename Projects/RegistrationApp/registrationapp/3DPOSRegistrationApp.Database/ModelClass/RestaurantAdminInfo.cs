using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Database.ModelClass
{
    public class RestaurantAdminInfo
    {
        private readonly IConfiguration _configuration;
        private readonly string Id;
        private readonly string FullName;
        private readonly string Email;
        private readonly string UserName;
        private readonly string NormalizedUserName;
        private readonly string NormalizedEmail;
        private readonly string UserStatus;
        private readonly string Password;
        private readonly string RoleId;
        private readonly string RoleName;
        private readonly string VerificationCode;

        public RestaurantAdminInfo(IConfiguration configuration)
        {
            this._configuration = configuration;
            this.Id = configuration.GetSection("RestaurantAdminInfo").GetSection("Id").Value;
            this.FullName = configuration.GetSection("RestaurantAdminInfo").GetSection("FullName").Value;
            this.Email = configuration.GetSection("RestaurantAdminInfo").GetSection("Email").Value;
            this.UserName = configuration.GetSection("RestaurantAdminInfo").GetSection("UserName").Value;
            this.NormalizedUserName = configuration.GetSection("RestaurantAdminInfo").GetSection("NormalizedUserName").Value;
            this.NormalizedEmail = configuration.GetSection("RestaurantAdminInfo").GetSection("NormalizedEmail").Value;
            this.UserStatus = configuration.GetSection("RestaurantAdminInfo").GetSection("UserStatus").Value;
            this.Password = configuration.GetSection("RestaurantAdminInfo").GetSection("Password").Value;
            this.RoleId = configuration.GetSection("RestaurantAdminInfo").GetSection("RoleId").Value;
            this.RoleName = configuration.GetSection("RestaurantAdminInfo").GetSection("RoleName").Value;
            this.VerificationCode = configuration.GetSection("RestaurantAdminInfo").GetSection("VerificationCode").Value;
        }

        public RestaurantUserInfoData StoreRestaurantUserInfoData()
        {
            RestaurantUserInfoData restaurantUserInfoData = new RestaurantUserInfoData();
            restaurantUserInfoData.Id = Id;
            restaurantUserInfoData.FullName = FullName;
            restaurantUserInfoData.Email = Email;
            restaurantUserInfoData.UserName = UserName;
            restaurantUserInfoData.NormalizedUserName = NormalizedUserName;
            restaurantUserInfoData.NormalizedEmail = NormalizedEmail;
            restaurantUserInfoData.UserStatus = UserStatus;
            restaurantUserInfoData.Password = Password;
            restaurantUserInfoData.RoleId = RoleId;
            restaurantUserInfoData.RoleName = RoleName;
            restaurantUserInfoData.VerificationCode = VerificationCode;

            return restaurantUserInfoData;
        }
    }
}
