using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Database.ModelClass
{
    public class Restaurant3DPOSInfo
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

        public Restaurant3DPOSInfo(IConfiguration configuration)
        {
            this._configuration = configuration;
            this.Id = configuration.GetSection("Restaurant3DPOSInfo").GetSection("Id").Value;
            this.FullName = configuration.GetSection("Restaurant3DPOSInfo").GetSection("FullName").Value;
            this.Email = configuration.GetSection("Restaurant3DPOSInfo").GetSection("Email").Value;
            this.UserName = configuration.GetSection("Restaurant3DPOSInfo").GetSection("UserName").Value;
            this.NormalizedUserName = configuration.GetSection("Restaurant3DPOSInfo").GetSection("NormalizedUserName").Value;
            this.NormalizedEmail = configuration.GetSection("Restaurant3DPOSInfo").GetSection("NormalizedEmail").Value;
            this.UserStatus = configuration.GetSection("Restaurant3DPOSInfo").GetSection("UserStatus").Value;
            this.Password = configuration.GetSection("Restaurant3DPOSInfo").GetSection("Password").Value;
            this.RoleId = configuration.GetSection("Restaurant3DPOSInfo").GetSection("RoleId").Value;
            this.RoleName = configuration.GetSection("Restaurant3DPOSInfo").GetSection("RoleName").Value;
            this.VerificationCode = configuration.GetSection("Restaurant3DPOSInfo").GetSection("VerificationCode").Value;
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
