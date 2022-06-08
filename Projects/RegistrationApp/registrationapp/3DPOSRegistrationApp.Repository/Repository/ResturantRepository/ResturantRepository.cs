using _3DPOSRegistrationApp.Database;
using CommonEntityModel.EntityModel;
using _3DPOSRegistrationApp.Repository.IRepository.ILogRepository;
using _3DPOSRegistrationApp.Repository.IRepository.IResturantRepository;
using _3DPOSRegistrationApp.Utility.CommonModel;
using _3DPOSRegistrationApp.Utility.ResturantModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using _3DPOSRegistrationApp.Database.Model;
using System.Security.Claims;
using _3DPOSRegistrationApp.Database.ModelClass;
using Microsoft.Extensions.Configuration;

namespace _3DPOSRegistrationApp.Repository.Repository.ResturantRepository
{
    public class ResturantRepository : IResturantRepository
    {
        private const string _areaName = "3DPOSRegistration Module";
        private const string _controllerName = "ResturantRepository";
        private readonly _3DPOS_DBContext _DBContext;
        private readonly RestaurantDbContext _restaurantDbContext;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public ResturantRepository(_3DPOS_DBContext DBContext, RestaurantDbContext restaurantDbContext, IErrorLogRepository errorLogRepository, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._roleManager = roleManager;
            this._userManager = userManager;
            this._restaurantDbContext = restaurantDbContext;
            this._configuration = configuration;
        }
        
        public Resturant SaveResturant(ResturantModel resturant, int loggedInUserId)
        {
            string actionName = "SaveDistrict";
            try
            {
                var resturantData = new Resturant();
                int resturantID = _DBContext.Resturant.Max(rq => (int?)rq.ResturantID) ?? 0;
                resturantID++;

                string resturantName1st3Letters = resturant.RestaurantName.ToString().Substring(0, 3).ToUpper();
                string resturantDistrict1st2Letters = resturant.District.ToString().Substring(0, 2).ToUpper();
                string resturantIDString = resturantID.ToString().PadLeft(3, '0');

                string ResturentCode = resturantName1st3Letters+""+ resturantIDString + DateTime.Now.Day + "" + DateTime.Now.Month+""+ resturantDistrict1st2Letters;

                bool isIntPresent = ResturentCode.ToString().Substring(0, 1).Any(char.IsDigit);

                if (isIntPresent == true)
                {
                    resturantData.ResturentCode ="_"+ResturentCode;
                }
                else
                {
                    resturantData.ResturentCode = ResturentCode;
                }
                
                resturantData.RestaurantName = resturant.RestaurantName;
                resturantData.CompanyRegistrationNo = resturant.CompanyRegistrationNo;
                resturantData.ContactPersion = resturant.ContactPersion;
                resturantData.ContactEmail = resturant.ContactEmail;
                resturantData.ContactPhone = resturant.ContactPhone;
                resturantData.Country = resturant.Country;
                resturantData.District = resturant.District;
                resturantData.PostCode = resturant.PostCode;
                resturantData.Address = resturant.Address;
                resturantData.IsDBCreated = false;
                resturantData.CreatedBy = loggedInUserId;
                resturantData.CreatedDate = DateTime.UtcNow;
                resturantData.DBCreationStatus = "Pending";
                resturantData.IsTableCreated = false;
                resturantData.DatabaseName = null;
                resturantData.DBConnectionString = null;
                resturantData.RestaurantStatus = resturant.RestaurantStatus;

                _DBContext.Resturant.Add(resturantData);
                _DBContext.SaveChanges();

                return resturantData;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<ResturantModel>> GetResturantList(int loggedInUserId)
        {
            string actionName = "GetResturantList";
            try
            {
                List<ResturantModel> resturentList = new List<ResturantModel>();

                var resturantData = 
                    (from R in _DBContext.Resturant
                     from U in _DBContext.Users.Where(U => U.Id== R.CreatedBy).DefaultIfEmpty()
                     
                     select new
                     {
                         R.ResturantID,
                         R.ResturentCode,
                         R.RestaurantName,
                         R.CompanyRegistrationNo,
                         R.ContactPersion,
                         R.ContactEmail,
                         R.ContactPhone,
                         R.Country,
                         R.District,
                         R.PostCode,
                         R.Address,
                         R.IsDBCreated,
                         R.IsTableCreated,
                         R.DatabaseName,
                         R.RestaurantUserId,
                         R.Email,
                         R.Password,
                         R.RestaurantStatus,
                         UserName = U.UserName == null ? "No User Logged In" : U.UserName
                     })
                     //.Where(x => x.ResturentCode.Contains().CountryName.Contains(SearchText.ToString()) || x.D.DistrictName.Contains(SearchText.ToString()))
                     .OrderByDescending(x => x.ResturantID).ToList();

                foreach (var data in resturantData)
                {
                    resturentList.Add(new ResturantModel
                    {
                         ResturantID= data.ResturantID,
                         ResturentCode =data.ResturentCode,
                         RestaurantName = data.RestaurantName,
                         CompanyRegistrationNo =data.CompanyRegistrationNo,
                         ContactPersion =data.ContactPersion,
                         ContactEmail =data.ContactEmail,
                         ContactPhone =data.ContactPhone,
                         Country =data.Country,
                         District =data.District,
                         PostCode =data.PostCode,
                         Address =data.Address,
                         CreatedBy =data.UserName,
                         IsDBCreated = data.IsDBCreated,
                         IsTableCreated=data.IsTableCreated,
                         DatabaseName=data.DatabaseName,
                         RestaurantUserId=data.RestaurantUserId,
                         Email=data.Email,
                         Password=data.Password,
                         RestaurantStatus=data.RestaurantStatus
                    });
                }

                return resturentList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<ResturantModel>> GetResturantListBySearchValue(string SearchText, int loggedInUserId)
        {
            string actionName = "GetResturantListBySearchValue";
            try
            {
                List<ResturantModel> resturentList = new List<ResturantModel>();

                var resturantData = 
                    (from R in _DBContext.Resturant
                     from U in _DBContext.Users.Where(U => U.Id == R.CreatedBy).DefaultIfEmpty()

                     select new
                     {
                         R.ResturantID,
                         R.ResturentCode,
                         R.RestaurantName,
                         R.CompanyRegistrationNo,
                         R.ContactPersion,
                         R.ContactEmail,
                         R.ContactPhone,
                         R.Country,
                         R.District,
                         R.PostCode,
                         R.IsDBCreated,
                         R.Address,
                         R.IsTableCreated,
                         R.DatabaseName,
                         R.RestaurantUserId,
                         R.Email,
                         R.Password,
                         R.RestaurantStatus,
                         UserName = U.UserName == null ? "No User Logged In" : U.UserName
                     })
                     .Where(x => x.ResturentCode.Contains(SearchText.ToString())||x.RestaurantName.Contains(SearchText.ToString()) || x.Email.Contains(SearchText.ToString()) || x.Country.Contains(SearchText.ToString()) || x.District.Contains(SearchText.ToString()) || x.ContactPhone.Contains(SearchText.ToString()) || x.UserName.Contains(SearchText.ToString()) || x.DatabaseName.Contains(SearchText.ToString()))
                     .OrderByDescending(x => x.ResturantID).ToList();

                foreach (var data in resturantData)
                {
                    resturentList.Add(new ResturantModel
                    {
                        ResturantID = data.ResturantID,
                        ResturentCode = data.ResturentCode,
                        RestaurantName = data.RestaurantName,
                        CompanyRegistrationNo = data.CompanyRegistrationNo,
                        ContactPersion = data.ContactPersion,
                        ContactEmail = data.ContactEmail,
                        ContactPhone = data.ContactPhone,
                        Country = data.Country,
                        District = data.District,
                        PostCode = data.PostCode,
                        Address = data.Address,
                        CreatedBy = data.UserName,
                        IsDBCreated=data.IsDBCreated,
                        IsTableCreated = data.IsTableCreated,
                        DatabaseName = data.DatabaseName,
                        RestaurantUserId = data.RestaurantUserId,
                        Email = data.Email,
                        Password = data.Password,
                        RestaurantStatus=data.RestaurantStatus
                    });
                }

                return resturentList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<ResturantModel> GetResturantInfoByID(int id, int loggedInUserId)
        {
            string actionName = "GetResturantByID";
            ResturantModel resturantModel = new ResturantModel();

            try
            {
                var resturant = _DBContext.Resturant.Where(x => x.ResturantID == id).FirstOrDefault();

                resturantModel.ResturantID = resturant.ResturantID;
                resturantModel.ResturentCode = resturant.ResturentCode;
                resturantModel.RestaurantName = resturant.RestaurantName;
                resturantModel.CompanyRegistrationNo = resturant.CompanyRegistrationNo;
                resturantModel.ContactPersion = resturant.ContactPersion;
                resturantModel.ContactEmail = resturant.ContactEmail;
                resturantModel.ContactPhone = resturant.ContactPhone;
                resturantModel.Country = resturant.Country;
                resturantModel.District = resturant.District;
                resturantModel.PostCode = resturant.PostCode;
                resturantModel.Address = resturant.Address;
                resturantModel.IsDBCreated = resturant.IsDBCreated;
                resturantModel.IsTableCreated = resturant.IsTableCreated;
                resturantModel.DatabaseName = resturant.DatabaseName;
                resturantModel.DBCreationStatus = resturant.DBCreationStatus;
                resturantModel.DBConnectionString = resturant.DBConnectionString;
                resturantModel.UserStatus = resturant.UserStatus;
                resturantModel.RestaurantUserId = resturant.RestaurantUserId;
                resturantModel.Email = resturant.Email;
                resturantModel.Password = resturant.Password;
                resturantModel.RestaurantStatus = resturant.RestaurantStatus;

                return resturantModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public bool AddDatabaseToRestaurant(ResturantModel resturantData, int loggedInUserId)
        {
            string actionName = "AddDatabaseToRestaurant";

            Resturant resturant = new Resturant();

            try
            {
                resturant.UpdatedBy = loggedInUserId;
                resturant.UpdatedDate = DateTime.UtcNow;
                resturant.IsDBCreated = true;
                resturant.DBCreationStatus = "Done";
                resturant.IsTableCreated = resturantData.IsTableCreated;
                resturant.RestaurantUserId = resturantData.RestaurantUserId;
                resturant.Email = resturantData.Email;
                resturant.Password = resturantData.Password;

                _DBContext.Resturant.Update(resturant);
                _DBContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return false;
            }
        }

        public async Task<List<RestaurantUsers>> RestaurantUserList(int loggedInUserId = 0, int restaurantID = 0)
        {
            List<RestaurantUsers> restaurantUsersList = new List<RestaurantUsers>();
            string actionName = "RestaurantUserList";
            try
            {
                Resturant resturant =  _DBContext.Resturant.Where(x => x.ResturantID == restaurantID).FirstOrDefault();

                var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>().UseSqlServer(resturant.DBConnectionString);
                var context = new RestaurantDbContext(optionsBuilder.Options);

                var restaurantUserList = context.ApplicationUser
                    .Join(context.UserRoles,AU=>AU.Id,UR=>UR.UserId,(AU,UR)=>new { AU,UR})
                    .Join(context.Roles,UR2=>UR2.UR.RoleId,R=>R.Id,(UR2,R)=>new { UR2,R})
                    .Where(x=>x.UR2.AU.RestaurantCode==resturant.ResturentCode)
                    .Select(x => new
                    {
                        x.UR2.AU.Id,
                        x.UR2.AU.UserName,
                        x.UR2.AU.Email,
                        x.UR2.AU.DefaultPassword,
                        x.UR2.AU.CreatedBy,
                        x.UR2.AU.CreatedDate,
                        x.R.Name
                    }).OrderByDescending(x=>x.Id).ToList();

                foreach(var data in restaurantUserList)
                {
                    RestaurantUsers restaurantUsers = new RestaurantUsers();

                    restaurantUsers.UserID = data.Id;
                    restaurantUsers.RestaurantCode = resturant.ResturentCode;
                    restaurantUsers.UserName = data.UserName;
                    restaurantUsers.Email = data.Email;
                    restaurantUsers.Password = data.DefaultPassword;
                    restaurantUsers.CreatedBy = data.CreatedBy;
                    restaurantUsers.CreatedDate =Convert.ToDateTime(data.CreatedDate);
                    restaurantUsers.RoleName = data.Name;

                    restaurantUsersList.Add(restaurantUsers);
                }

                return restaurantUsersList;
                //var connectionString = resturant.DBConnectionString.ToString();
                //DataTable dataTable = new DataTable();

                //using (SqlConnection connection = new SqlConnection(connectionString))
                //{
                //    try
                //    {
                //        string sqlQuery = @"select * from RestaurantUser order by Id desc";
                //        using (SqlCommand cmd = new SqlCommand(sqlQuery, connection))
                //        {
                //            cmd.CommandTimeout = 0;
                //            cmd.CommandType = CommandType.StoredProcedure;
                //            connection.Open();
                //            SqlDataAdapter da = new SqlDataAdapter(cmd);
                //            da.Fill(dataTable);
                //        }
                //    }
                //    catch (Exception ex)
                //    {

                //    }
                //    connection.Close();
                //}
                //return resturant;

            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<RestaurantUsers>> RestaurantUserListBySearchValue(string SearchText = "", int loggedInUserId = 0, int restaurantID = 0)
        {
            List<RestaurantUsers> restaurantUsersList = new List<RestaurantUsers>();
            string actionName = "RestaurantUserList";
            try
            {
                Resturant resturant =  _DBContext.Resturant.Where(x => x.ResturantID == restaurantID).FirstOrDefault();

                var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>().UseSqlServer(resturant.DBConnectionString);
                var context = new RestaurantDbContext(optionsBuilder.Options);

                var restaurantUserList = context.ApplicationUser
                    .Join(context.UserRoles, AU => AU.Id, UR => UR.UserId, (AU, UR) => new { AU, UR })
                    .Join(context.Roles, UR2 => UR2.UR.RoleId, R => R.Id, (UR2, R) => new { UR2, R })
                    .Where(x => x.UR2.AU.Email.Contains(SearchText.ToString()) || x.UR2.AU.UserName.Contains(SearchText.ToString()) || resturant.ResturentCode.Contains(SearchText.ToString()) && x.UR2.AU.RestaurantCode == resturant.ResturentCode)
                    .Select(x => new
                    {
                        x.UR2.AU.Id,
                        x.UR2.AU.UserName,
                        x.UR2.AU.Email,
                        x.UR2.AU.DefaultPassword,
                        x.UR2.AU.CreatedBy,
                        x.UR2.AU.CreatedDate,
                        x.R.Name
                    }).OrderByDescending(x => x.Id).ToList();

                foreach (var data in restaurantUserList)
                {
                    RestaurantUsers restaurantUsers = new RestaurantUsers();

                    restaurantUsers.UserID = data.Id;
                    restaurantUsers.RestaurantCode = resturant.ResturentCode;
                    restaurantUsers.UserName = data.UserName;
                    restaurantUsers.Email = data.Email;
                    restaurantUsers.Password = data.DefaultPassword;
                    restaurantUsers.CreatedBy = data.CreatedBy;
                    restaurantUsers.CreatedDate =Convert.ToDateTime(data.CreatedDate);
                    restaurantUsers.RoleName = data.Name;

                    restaurantUsersList.Add(restaurantUsers);
                }

                return restaurantUsersList;

            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<RestaurantUserRole>> GetRestaurantUserRoles(int loggedInUserId = 0, int id = 0)
        {
            List<RestaurantUserRole> restaurantUserRoles = new List<RestaurantUserRole>();
            string actionName = "GetRestaurantUserRoles";
            try
            {
                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturantID == id).FirstOrDefault();

                var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>().UseSqlServer(resturant.DBConnectionString);
                var context = new RestaurantDbContext(optionsBuilder.Options);

                var restaurantUserRoleList = context.Roles.OrderByDescending(x => x.Id).ToList();

                foreach (var data in restaurantUserRoleList)
                {
                    RestaurantUserRole restaurantUserRole = new RestaurantUserRole();

                    restaurantUserRole.RoleId = data.Id;
                    restaurantUserRole.RoleName = data.Name;

                    restaurantUserRoles.Add(restaurantUserRole);
                }

                return restaurantUserRoles;

            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }


        public async Task<RestaurantUserWithDatabase> SaveNewRestaurantUser(RestaurantUserWithDatabase restaurantUserWithDatabase, int loggedInUserId = 0)
        {
            List<RestaurantUserRole> restaurantUserRoles = new List<RestaurantUserRole>();
            string actionName = "SaveNewRestaurantUser";
            try
            {
                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturantID == restaurantUserWithDatabase.RestaurantId).FirstOrDefault();

                var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>().UseSqlServer(resturant.DBConnectionString);
                
                using (var context = new RestaurantDbContext(optionsBuilder.Options))
                {
                    using (IDbContextTransaction transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            PasswordHasher<ApplicationUser> ph = new PasswordHasher<ApplicationUser>();

                            var restaurantNewUser = new ApplicationUser
                            {
                                UserName = restaurantUserWithDatabase.UserName,
                                NormalizedUserName = (restaurantUserWithDatabase.UserName).ToUpper(),
                                Email = restaurantUserWithDatabase.Email.ToLower(),
                                NormalizedEmail = (restaurantUserWithDatabase.Email).ToUpper(),
                                UserStatus = 1,
                                EmailConfirmed = true,
                                CreatedBy = loggedInUserId,
                                CreatedDate = DateTime.UtcNow,
                                LockoutEnabled = true,
                                RestaurantCode = resturant.ResturentCode,
                                DefaultPassword= restaurantUserWithDatabase.Password
                            };
                            restaurantNewUser.PasswordHash = ph.HashPassword(restaurantNewUser, restaurantUserWithDatabase.Password);
                            restaurantNewUser.SecurityStamp = Guid.NewGuid().ToString();
                            context.ApplicationUser.Add(restaurantNewUser);
                            context.SaveChanges();

                            UserClaim restaurantNewUserClaim = new UserClaim();
                            restaurantNewUserClaim.UserId = restaurantNewUser.Id;
                            restaurantNewUserClaim.ClaimType = "RestaurantCode";
                            restaurantNewUserClaim.ClaimValue = resturant.ResturentCode;
                            context.UserClaims.Add(restaurantNewUserClaim);
                            context.SaveChanges();

                            Microsoft.AspNetCore.Identity.IdentityUserRole<int> restaurantNewUserRole = new Microsoft.AspNetCore.Identity.IdentityUserRole<int>();
                            restaurantNewUserRole.RoleId = restaurantUserWithDatabase.RoleId;
                            restaurantNewUserRole.UserId = restaurantNewUser.Id;
                            context.UserRoles.Add(restaurantNewUserRole);
                            context.SaveChanges();

                            transaction.Commit();

                            return restaurantUserWithDatabase;
                        }
                        catch (Exception ex)
                        {
                            bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                            transaction.Rollback();
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }


        public async Task<string> CheckExistRestaurantUser(RestaurantUserWithDatabase restaurantUserWithDatabase, int loggedInUserId = 0)
        {
            List<RestaurantUserRole> restaurantUserRoles = new List<RestaurantUserRole>();
            string actionName = "CheckExistRestaurantUser";
            try
            {
                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturantID == restaurantUserWithDatabase.RestaurantId).FirstOrDefault();

                var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>().UseSqlServer(resturant.DBConnectionString);

                using (var context = new RestaurantDbContext(optionsBuilder.Options))
                {
                    var existUser =await context.Users.Where(x => x.RestaurantCode == resturant.ResturentCode && x.UserName == restaurantUserWithDatabase.UserName && x.Email==restaurantUserWithDatabase.Email).FirstOrDefaultAsync();

                    if (existUser != null)
                    {
                        return "True";
                    }
                    else
                    {
                        return "False";
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return "Error";
            }
        }

        //public bool CreateRestaurantUser(string restaurantCode, int loggedInUserId, string dbConn)
        //{
        //    string actionName = "CreateRestaurantUser";
        //    try
        //    {
        //        var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>().UseSqlServer(dbConn);

        //        using (var context = new RestaurantDbContext(optionsBuilder.Options))
        //        {
        //            ////////////Restaurant user add in DB-Catalog database

        //            PasswordHasher<ApplicationUser> ph = new PasswordHasher<ApplicationUser>();
        //            var restaurantUser = new ApplicationUser
        //            {
        //                UserName = restaurantCode + "@admin.com",
        //                NormalizedUserName = restaurantCode + "@admin.com".ToUpper(),
        //                Email = restaurantCode + "@admin.com",
        //                NormalizedEmail = restaurantCode + "@admin.com".ToUpper(),
        //                UserStatus = 1,
        //                EmailConfirmed = true,
        //                CreatedBy = loggedInUserId,
        //                CreatedDate = DateTime.UtcNow,
        //                LockoutEnabled = true,
        //                RestaurantCode= restaurantCode
        //            };
        //            restaurantUser.PasswordHash = ph.HashPassword(restaurantUser, "asdf1234");
        //            restaurantUser.SecurityStamp = Guid.NewGuid().ToString();
        //            context.ApplicationUser.Add(restaurantUser);

        //            context.SaveChanges();

        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //        return false;
        //    }
        //}

        public async Task<Resturant> CreateRestaurantAndRestaurantUserInDBCatalogDatabase(ResturantModel resturantModel, int loggedInUserId)
        {
            string actionName = "CreateRestaurantAndRestaurantUserInDBCatalogDatabase";
            Resturant resturant = new Resturant();
            try
            {
                RestaurantSupperAdminInfo restaurantSupperAdminInfo = new RestaurantSupperAdminInfo(_configuration);
                RestaurantAdminInfo restaurantAdminInfo = new RestaurantAdminInfo(_configuration);
                Restaurant3DPOSInfo restaurant3DPOSInfo = new Restaurant3DPOSInfo(_configuration);
                RestaurantUserInfoData restaurantSupperAdminInfoData = restaurantSupperAdminInfo.StoreRestaurantUserInfoData();
                RestaurantUserInfoData restaurantAdminInfoData = restaurantAdminInfo.StoreRestaurantUserInfoData();
                RestaurantUserInfoData restaurant3DPOSInfoData = restaurant3DPOSInfo.StoreRestaurantUserInfoData();



                var dbCatalog =  _DBContext.DBCatalog.Where(x => x.ID == resturantModel.DBCatalogID).FirstOrDefault();
                var resturantData =  _DBContext.Resturant.Where(x => x.ResturantID == resturantModel.ResturantID).FirstOrDefault();
                var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>().UseSqlServer(dbCatalog.ConnectionString);
                
                using (var context = new RestaurantDbContext(optionsBuilder.Options))
                {
                    using (IDbContextTransaction transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            ////////////RestaurantSupperAdmin

                            PasswordHasher<ApplicationUser> ph = new PasswordHasher<ApplicationUser>();

                            var restaurantSupperAdmin = new ApplicationUser
                            {
                                UserName = resturantData.ResturentCode + restaurantSupperAdminInfoData.UserName,
                                NormalizedUserName = (resturantData.ResturentCode + restaurantSupperAdminInfoData.NormalizedUserName).ToUpper(),
                                Email = resturantData.ResturentCode + restaurantSupperAdminInfoData.Email.ToLower(),
                                NormalizedEmail = (resturantData.ResturentCode + restaurantSupperAdminInfoData.NormalizedEmail).ToUpper(),
                                UserStatus = 1,
                                EmailConfirmed = true,
                                CreatedBy = loggedInUserId,
                                CreatedDate = DateTime.UtcNow,
                                LockoutEnabled = true,
                                RestaurantCode = resturantData.ResturentCode,
                                DefaultPassword= resturantData.ResturentCode + restaurantSupperAdminInfoData.Password 
                            };
                            restaurantSupperAdmin.PasswordHash = ph.HashPassword(restaurantSupperAdmin, resturantData.ResturentCode + restaurantSupperAdminInfoData.Password);
                            restaurantSupperAdmin.SecurityStamp = Guid.NewGuid().ToString();
                            context.ApplicationUser.Add(restaurantSupperAdmin);
                            context.SaveChanges();

                            UserClaim restaurantSupperAdminClaim = new UserClaim();
                            restaurantSupperAdminClaim.UserId = restaurantSupperAdmin.Id;
                            restaurantSupperAdminClaim.ClaimType = "RestaurantCode";
                            restaurantSupperAdminClaim.ClaimValue = resturantData.ResturentCode;
                            context.UserClaims.Add(restaurantSupperAdminClaim);
                            context.SaveChanges();

                            Microsoft.AspNetCore.Identity.IdentityUserRole<int> restaurantSupperAdminClaimRole = new Microsoft.AspNetCore.Identity.IdentityUserRole<int>();
                            restaurantSupperAdminClaimRole.RoleId = 1;
                            restaurantSupperAdminClaimRole.UserId = restaurantSupperAdmin.Id;
                            context.UserRoles.Add(restaurantSupperAdminClaimRole);

                            VarificationCode restaurantSupperAdminVerificationCode = new VarificationCode();
                            restaurantSupperAdminVerificationCode.Code = restaurantSupperAdminInfoData.VerificationCode;
                            restaurantSupperAdminVerificationCode.Username = restaurantSupperAdmin.UserName;
                            restaurantSupperAdminVerificationCode.RestaurantCode = restaurantSupperAdmin.RestaurantCode;
                            restaurantSupperAdminVerificationCode.Status = true;
                            restaurantSupperAdminVerificationCode.IsDeleted = false;
                            restaurantSupperAdminVerificationCode.CreatedBy = loggedInUserId;
                            restaurantSupperAdminVerificationCode.CreatedDate = DateTime.UtcNow;
                            context.VarificationCode.Add(restaurantSupperAdminVerificationCode);
                            context.SaveChanges();


                            //////////RestaurantAdmin

                            var restaurantAdmin = new ApplicationUser
                            {
                                UserName = resturantData.ResturentCode + restaurantAdminInfoData.UserName,
                                NormalizedUserName = (resturantData.ResturentCode + restaurantAdminInfoData.NormalizedUserName).ToUpper(),
                                Email = resturantData.ResturentCode + restaurantAdminInfoData.Email.ToLower(),
                                NormalizedEmail = (resturantData.ResturentCode + restaurantAdminInfoData.NormalizedEmail).ToUpper(),
                                UserStatus = 1,
                                EmailConfirmed = true,
                                CreatedBy = loggedInUserId,
                                CreatedDate = DateTime.UtcNow,
                                LockoutEnabled = true,
                                RestaurantCode = resturantData.ResturentCode,
                                DefaultPassword = resturantData.ResturentCode + restaurantAdminInfoData.Password
                            };
                            restaurantAdmin.PasswordHash = ph.HashPassword(restaurantAdmin, resturantData.ResturentCode + restaurantAdminInfoData.Password);
                            restaurantAdmin.SecurityStamp = Guid.NewGuid().ToString();
                            context.ApplicationUser.Add(restaurantAdmin);
                            context.SaveChanges();

                            UserClaim restaurantAdminClaim = new UserClaim();
                            restaurantAdminClaim.UserId = restaurantAdmin.Id;
                            restaurantAdminClaim.ClaimType = "RestaurantCode";
                            restaurantAdminClaim.ClaimValue = resturantData.ResturentCode;
                            context.UserClaims.Add(restaurantAdminClaim);
                            context.SaveChanges();

                            Microsoft.AspNetCore.Identity.IdentityUserRole<int> restaurantAdminRole = new Microsoft.AspNetCore.Identity.IdentityUserRole<int>();
                            restaurantAdminRole.RoleId = 2;
                            restaurantAdminRole.UserId = restaurantAdmin.Id;
                            context.UserRoles.Add(restaurantAdminRole);

                            VarificationCode restaurantAdminVerificationCode = new VarificationCode();
                            restaurantAdminVerificationCode.Code = restaurantAdminInfoData.VerificationCode;
                            restaurantAdminVerificationCode.Username = restaurantAdmin.UserName;
                            restaurantAdminVerificationCode.RestaurantCode = restaurantAdmin.RestaurantCode;
                            restaurantAdminVerificationCode.Status = true;
                            restaurantAdminVerificationCode.IsDeleted = false;
                            restaurantAdminVerificationCode.CreatedBy = loggedInUserId;
                            restaurantAdminVerificationCode.CreatedDate = DateTime.UtcNow;
                            context.VarificationCode.Add(restaurantAdminVerificationCode);
                            context.SaveChanges();


                            ////////////Restaurant3DPOSAdmin


                            var restaurant3DPOSAdmin = new ApplicationUser
                            {
                                UserName = resturantData.ResturentCode + restaurant3DPOSInfoData.UserName,
                                NormalizedUserName = (resturantData.ResturentCode + restaurant3DPOSInfoData.NormalizedUserName).ToUpper(),
                                Email = resturantData.ResturentCode + restaurant3DPOSInfoData.Email.ToLower(),
                                NormalizedEmail = (resturantData.ResturentCode + restaurant3DPOSInfoData.NormalizedEmail).ToUpper(),
                                UserStatus = 1,
                                EmailConfirmed = true,
                                CreatedBy = loggedInUserId,
                                CreatedDate = DateTime.UtcNow,
                                LockoutEnabled = true,
                                RestaurantCode = resturantData.ResturentCode,
                                DefaultPassword = resturantData.ResturentCode + restaurant3DPOSInfoData.Password
                            };
                            restaurant3DPOSAdmin.PasswordHash = ph.HashPassword(restaurant3DPOSAdmin, resturantData.ResturentCode + restaurant3DPOSInfoData.Password);
                            restaurant3DPOSAdmin.SecurityStamp = Guid.NewGuid().ToString();
                            context.ApplicationUser.Add(restaurant3DPOSAdmin);
                            context.SaveChanges();

                            UserClaim restaurant3DPOSAdminClaim = new UserClaim();
                            restaurant3DPOSAdminClaim.UserId = restaurant3DPOSAdmin.Id;
                            restaurant3DPOSAdminClaim.ClaimType = "RestaurantCode";
                            restaurant3DPOSAdminClaim.ClaimValue = resturantData.ResturentCode;
                            context.UserClaims.Add(restaurant3DPOSAdminClaim);
                            context.SaveChanges();

                            Microsoft.AspNetCore.Identity.IdentityUserRole<int> restaurant3DPOSAdminRole = new Microsoft.AspNetCore.Identity.IdentityUserRole<int>();
                            restaurant3DPOSAdminRole.RoleId = 3;
                            restaurant3DPOSAdminRole.UserId = restaurant3DPOSAdmin.Id;
                            context.UserRoles.Add(restaurant3DPOSAdminRole);

                            VarificationCode restaurant3DPOSAdminVerificationCode = new VarificationCode();
                            restaurant3DPOSAdminVerificationCode.Code = restaurant3DPOSInfoData.VerificationCode;
                            restaurant3DPOSAdminVerificationCode.Username = restaurant3DPOSAdmin.UserName;
                            restaurant3DPOSAdminVerificationCode.RestaurantCode = restaurant3DPOSAdmin.RestaurantCode;
                            restaurant3DPOSAdminVerificationCode.Status = true;
                            restaurant3DPOSAdminVerificationCode.IsDeleted = false;
                            restaurant3DPOSAdminVerificationCode.CreatedBy = loggedInUserId;
                            restaurant3DPOSAdminVerificationCode.CreatedDate = DateTime.UtcNow;
                            context.VarificationCode.Add(restaurant3DPOSAdminVerificationCode);
                            context.SaveChanges();


                            resturant.ResturentCode = resturantData.ResturentCode;
                            resturant.RestaurantName = resturantModel.RestaurantName;
                            resturant.CompanyRegistrationNo = resturantModel.CompanyRegistrationNo;
                            resturant.ContactPersion = resturantModel.ContactPersion;
                            resturant.ContactEmail = resturantModel.ContactEmail;
                            resturant.ContactPhone = resturantModel.ContactPhone;
                            resturant.Country = resturantModel.Country;
                            resturant.District = resturantModel.District;
                            resturant.PostCode = resturantModel.PostCode;
                            resturant.Address = resturantModel.Address;
                            resturant.IsDBCreated = true;
                            resturant.IsTableCreated = true;
                            resturant.DBCreationStatus = "Done";
                            resturant.DBConnectionString = dbCatalog.ConnectionString;
                            resturant.DatabaseName = dbCatalog.DatabaseName;
                            resturant.CreatedBy = loggedInUserId;
                            resturant.CreatedDate = DateTime.UtcNow;
                            //resturant.UserStatus = resturantModel.UserStatus;
                            //resturant.RestaurantUserId = resturantData.ResturentCode;
                            //resturant.Email = resturantData.ResturentCode + "@admin.com";
                            //resturant.Password = "asdf1234";
                            resturant.RestaurantStatus = resturantModel.RestaurantStatus;
                            context.Resturant.Add(resturant);


                            resturantData.ResturentCode = resturantData.ResturentCode;
                            resturantData.RestaurantName = resturantModel.RestaurantName;
                            resturantData.CompanyRegistrationNo = resturantModel.CompanyRegistrationNo;
                            resturantData.ContactPersion = resturantModel.ContactPersion;
                            resturantData.ContactEmail = resturantModel.ContactEmail;
                            resturantData.ContactPhone = resturantModel.ContactPhone;
                            resturantData.Country = resturantModel.Country;
                            resturantData.District = resturantModel.District;
                            resturantData.PostCode = resturantModel.PostCode;
                            resturantData.Address = resturantModel.Address;
                            resturantData.IsDBCreated = true;
                            resturantData.IsTableCreated = true;
                            resturantData.DBCreationStatus = "Done";
                            resturantData.DBConnectionString = dbCatalog.ConnectionString;
                            resturantData.DatabaseName = dbCatalog.DatabaseName;
                            resturantData.UpdatedBy = loggedInUserId;
                            resturantData.UpdatedDate = DateTime.UtcNow;
                            //resturant.UserStatus = resturantModel.UserStatus;
                            //resturantData.RestaurantUserId = resturantData.ResturentCode;
                            //resturantData.Email = resturantData.ResturentCode + "@admin.com";
                            //resturantData.Password = "asdf1234";
                            resturantData.RestaurantStatus = resturantModel.RestaurantStatus;

                            context.SaveChanges();

                            var restaurantCount = context.Resturant.Count();
                            if( restaurantCount != null && restaurantCount > 0)
                            {
                                dbCatalog.RestaurantCount = dbCatalog.RestaurantCount + 1;
                            }
                            _DBContext.SaveChanges();

                            transaction.Commit();
                            return resturant;
                        }
                        catch (Exception ex)
                        {
                            bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                            transaction.Rollback();
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<Resturant> UpdateRestaurantInformation(ResturantModel resturantModel, int loggedInUserId)
        {
            string actionName = "UpdateRestaurantInformation";
            Resturant resturant = new Resturant();
            try
            {
                var resturantData =  _DBContext.Resturant.Where(x => x.ResturantID == resturantModel.ResturantID).FirstOrDefault();
                
                resturantData.RestaurantName = resturantModel.RestaurantName;
                resturantData.CompanyRegistrationNo = resturantModel.CompanyRegistrationNo;
                resturantData.ContactPersion = resturantModel.ContactPersion;
                resturantData.ContactEmail = resturantModel.ContactEmail;
                resturantData.ContactPhone = resturantModel.ContactPhone;
                resturantData.Country = resturantModel.Country;
                resturantData.District = resturantModel.District;
                resturantData.PostCode = resturantModel.PostCode;
                resturantData.Address = resturantModel.Address;
                resturantData.UpdatedBy = loggedInUserId;
                resturantData.UpdatedDate = DateTime.UtcNow;
                resturantData.RestaurantStatus = resturantModel.RestaurantStatus;
                //resturant.UserStatus = resturantModel.UserStatus;

                _DBContext.SaveChanges();
                return resturantData;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<bool> RemoveRestaurant(int id, int loggedInUserId)
        {
            string actionName = "RemoveRestaurant";

            try
            {
                var restaurant = await _DBContext.Resturant.Where(x => x.ResturantID == id).FirstOrDefaultAsync();

                _DBContext.Remove(restaurant);
                _DBContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return false ;
            }
        }

    }
}
