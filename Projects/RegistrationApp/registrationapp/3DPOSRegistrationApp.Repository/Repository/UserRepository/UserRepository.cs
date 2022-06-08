using _3DPOSRegistrationApp.Database;
using _3DPOSRegistrationApp.Repository.IRepository.ILogRepository;
using _3DPOSRegistrationApp.Repository.IRepository.IUserRepository;
using _3DPOSRegistrationApp.Utility.UserModel_View;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.Repository.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private const string _areaName = "3DPOSRegistration Module";
        private const string _controllerName = "UserRepository";
        private readonly _3DPOS_DBContext _DBContext;
        private IConfiguration _configuration;
        private readonly IErrorLogRepository _errorLogRepository;

        public UserRepository(_3DPOS_DBContext DBContext,IConfiguration configuration, IErrorLogRepository errorLogRepository)
        {
            _DBContext = DBContext;
            _configuration = configuration;
            this._errorLogRepository = errorLogRepository;
        }
        public async Task<List<UserViewModel>> GetUserList(int loggedInUserId)
        {
            string actionName = "GetUserList";
            var loggedInUserID = loggedInUserId;

            try
            {
                List<UserViewModel> userViewModel = new List<UserViewModel>();
                var userData = _DBContext.Users
                     .Join(_DBContext.UserRoles, U => U.Id, UR => UR.UserId, (U, UR) => new { U, UR })
                     .Join(_DBContext.Roles, UR2 => UR2.UR.RoleId, R => R.Id, (UR2, R) => new { UR2, R })
                     .Join(_DBContext.UserStatus, U2 => U2.UR2.U.UserStatus, US => US.UserStatusId, (U2, US) => new { U2, US })
                     .Select(x => new
                     {
                         UserId = x.U2.UR2.U.Id,
                         UserName = x.U2.UR2.U.UserName,
                         RoleId = x.U2.R.Id,
                         RoleName = x.U2.R.Name,
                         FullName = x.U2.UR2.U.FullName,
                         Email = x.U2.UR2.U.Email,
                         CreatedDate = x.U2.UR2.U.CreatedDate,
                         UpdatedBy = x.U2.UR2.U.UpdatedBy,
                         UpdatedDate = x.U2.UR2.U.UpdatedDate,
                         UserStatus = x.US.StatusType
                     }).OrderByDescending(x => x.UserId).ToList();

                foreach (var data in userData)
                {
                    userViewModel.Add(new UserViewModel
                    {
                        UserID = data.UserId,
                        Username = data.UserName,
                        UserRoleID = data.RoleId,
                        UserRole = data.RoleName,
                        FullName = data.FullName,
                        Email = data.Email,
                        CreatedDate = Convert.ToDateTime(data.CreatedDate),
                        UpdatedBy = data.UpdatedBy,
                        UpdatedDate = Convert.ToDateTime(data.UpdatedDate),
                        UserStatus = data.UserStatus
                    });
                }
                return userViewModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<UserViewModel>> GetUserListBySearchValue(string SearchText, int loggedInUserId)
        {
            string actionName = "GetUserListBySearchValue";
            var loggedInUserID = loggedInUserId;
            try
            {
                List<UserViewModel> userViewModel = new List<UserViewModel>();
                var userData = _DBContext.Users
                     .Join(_DBContext.UserRoles, U => U.Id, UR => UR.UserId, (U, UR) => new { U, UR })
                     .Join(_DBContext.Roles, UR2 => UR2.UR.RoleId, R => R.Id, (UR2, R) => new { UR2, R })
                     .Join(_DBContext.UserStatus, U2=> U2.UR2.U.UserStatus, US => US.UserStatusId, (U2, US) => new { U2, US })
                     .Where(x => x.U2.UR2.U.Email.Contains(SearchText.ToString()) || x.U2.UR2.U.UserName.Contains(SearchText.ToString()) || x.U2.R.Name.Contains(SearchText.ToString()) || x.US.StatusType.Contains(SearchText.ToString()) || x.U2.UR2.U.FullName.Contains(SearchText.ToString()))
                     .Select(x => new
                     {
                         UserId = x.U2.UR2.U.Id,
                         UserName = x.U2.UR2.U.UserName,
                         RoleId = x.U2.R.Id,
                         RoleName = x.U2.R.Name,
                         FullName = x.U2.UR2.U.FullName,
                         Email = x.U2.UR2.U.Email,
                         CreatedDate = x.U2.UR2.U.CreatedDate,
                         UpdatedBy = x.U2.UR2.U.UpdatedBy,
                         UpdatedDate = x.U2.UR2.U.UpdatedDate,
                         UserStatus=x.US.StatusType
                     }).OrderByDescending(x => x.UserId).ToList();

                foreach (var data in userData)
                {
                    userViewModel.Add(new UserViewModel
                    {
                        UserID = data.UserId,
                        Username = data.UserName,
                        UserRoleID = data.RoleId,
                        UserRole = data.RoleName,
                        FullName = data.FullName,
                        Email = data.Email,
                        CreatedDate = Convert.ToDateTime(data.CreatedDate),
                        UpdatedBy = data.UpdatedBy,
                        UpdatedDate = Convert.ToDateTime(data.UpdatedDate),
                        UserStatus=data.UserStatus
                        
                    });
                }
                return userViewModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public string GetUserRoleByUserId(int id, int loggedInUserId)
        {
            string actionName = "GetUserRoleByUserId";
            var loggedInUserID = loggedInUserId;
            try
            {
                var getUerRoleId = _DBContext.Users
                 .Join(_DBContext.UserRoles, U => U.Id, UR => UR.UserId, (U, UR) => new { U, UR })
                 .Join(_DBContext.Roles, UR2 => UR2.UR.RoleId, R => R.Id, (UR2, R) => new { UR2, R })
                 .Where(x => x.UR2.U.Id == id)
                 .Select(x => new
                 {
                     RoleName = x.R.Name,

                 }).FirstOrDefault();

                string userRole = getUerRoleId.RoleName;
                return userRole;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public RegistrationViewModel GetUser(int userId, int loggedInUserId)
        {
            RegistrationViewModel registrationViewModel = new RegistrationViewModel();
            string actionName = "GetUser";

            try
            {
                var userData = _DBContext.Users
                     .Join(_DBContext.UserRoles, U => U.Id, UR => UR.UserId, (U, UR) => new { U, UR })
                     .Join(_DBContext.Roles, UR2 => UR2.UR.RoleId, R => R.Id, (UR2, R) => new { UR2, R })
                     .Where(x => x.UR2.U.Id == userId)
                     .Select(x => new
                     {
                         x.UR2.U.Id,
                         x.UR2.U.UserStatus,
                         x.R.Name,
                         x.UR2.U.Email
                     }).FirstOrDefault();

                registrationViewModel.id = userData.Id;
                registrationViewModel.UserRole = userData.Name;
                registrationViewModel.Email = userData.Email;
                registrationViewModel.UserStatusId = userData.UserStatus;

                return registrationViewModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        //public string Login(LoginViewModel loginModel)
        //{
        //    if (loginModel != null)
        //    {
        //        //var userData = _DBContext.ApplicationUser.Where(x => x.Email == loginModel.Email && x.Password == loginModel.Password).FirstOrDefault();
        //        //if (userData != null)
        //        //{
        //        //    var token = GenerateJSONWebToken(loginModel);
        //        //    return token;
        //        //}
        //        //else
        //        //{
        //        //    return null;
        //        //}
        //        return null;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public string GenerateJSONWebToken(LoginViewModel loginModel, int loggedInUserId)
        {
            string actionName = "GenerateJSONWebToken";
            var loggedInUserID = loggedInUserId;
            try
            {
                //string key = "my_secret_key_12345";
                //var issuer = "http://mysite.com";
                //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                //var credential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                //var permClaims = new List<Claim>();
                //permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                //permClaims.Add(new Claim("valid", "1"));
                //permClaims.Add(new Claim("Email", loginModel.Email));
                //permClaims.Add(new Claim("Password", loginModel.Password));

                //var token = new JwtSecurityToken(issuer,
                //                issuer,
                //                permClaims,
                //                expires: DateTime.Now.AddDays(1),
                //                signingCredentials: credential);
                //var encodToken = new JwtSecurityTokenHandler().WriteToken(token);



                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var Claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub,loginModel.Password),
                new Claim(JwtRegisteredClaimNames.Email,loginModel.Email),
                 new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                };
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Issuer"],
                    Claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials);
                var encodToken = new JwtSecurityTokenHandler().WriteToken(token);
                return encodToken;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }
    }
}
