using _3DPOSRegistrationApp.Database.Model;
using _3DPOSRegistrationApp.Utility;
using _3DPOSRegistrationApp.Utility.UserModel_View;
using _3DPOSRegistrationApp.Utility.UserRoleModel;
using _3DPOSRegistrationApp.Utility.UserStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.IRepository.IUserRepository
{
    public interface IUserRepository
    {
        Task<List<UserViewModel>> GetUserList(int loggedInUserId);
        Task<List<UserViewModel>> GetUserListBySearchValue(string SearchText, int loggedInUserId);
        RegistrationViewModel GetUser(int userId, int loggedInUserId);
        string GenerateJSONWebToken(LoginViewModel loginModel, int loggedInUserId);
        string GetUserRoleByUserId(int id, int loggedInUserId);
        
    }
}
