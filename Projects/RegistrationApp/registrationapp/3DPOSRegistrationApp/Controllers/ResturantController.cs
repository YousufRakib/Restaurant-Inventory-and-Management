using _3DPOSRegistrationApp.Database;
using _3DPOSRegistrationApp.Database.Model;
using CommonEntityModel.EntityModel;
using _3DPOSRegistrationApp.Database.ModelClass;
using _3DPOSRegistrationApp.Repository.IRepository.ICommonRepository;
using _3DPOSRegistrationApp.Repository.IRepository.ILogRepository;
using _3DPOSRegistrationApp.Repository.IRepository.IResturantRepository;
using _3DPOSRegistrationApp.Repository.IRepository.IUserRepository;
using _3DPOSRegistrationApp.Utility.CommonModel;
using _3DPOSRegistrationApp.Utility.ResturantModel;
using _3DPOSRegistrationApp.Utility.ServiceModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CommonEntityModel.ModelClass;

namespace _3DPOSRegistrationApp.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class ResturantController : Controller
    {
        private const string _areaName = "3DPOSRegistration Module";
        private const string _controllerName = "Resturant";
        private readonly _3DPOS_DBContext _DBContext;
        private readonly RestaurantDbContext _restaurantDbContext;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserManager<AspNetUsers> _restaurantUserManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ResponseMessage _responseMessage = new ResponseMessage();
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IRoleLogRepository _roleLogRepository;
        private readonly IStatusLogRepository _statusLogRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly IResturantRepository _resturantRepository;
        private readonly IDBCatalogRepository _DBCatalogRepository;
        private readonly ReturnTypeModel ReturnTypeModel = new ReturnTypeModel();
        //private readonly string DataSource;
        //private readonly string UserId;
        //private readonly string Password;

        public ResturantController(_3DPOS_DBContext DBContext, RestaurantDbContext restaurantDbContext, IUserRepository userRepository, IErrorLogRepository errorLogRepository, IAuditLogRepository auditLogRepository, IRoleLogRepository roleLogRepository, IStatusLogRepository statusLogRepository, ICommonRepository commonRepository, IResturantRepository resturantRepository, IDBCatalogRepository DBCatalogRepository, IConfiguration configuration, UserManager<ApplicationUser> userManager, UserManager<AspNetUsers> restaurantUserManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            this._DBContext = DBContext;
            this._restaurantDbContext = restaurantDbContext;
            this._userRepository = userRepository;
            this._errorLogRepository = errorLogRepository;
            this._auditLogRepository = auditLogRepository;
            this._roleLogRepository = roleLogRepository;
            this._statusLogRepository = statusLogRepository;
            this._commonRepository = commonRepository;
            this._resturantRepository = resturantRepository;
            this._DBCatalogRepository = DBCatalogRepository;
            this._configuration = configuration;
            this._userManager = userManager;
            this._restaurantUserManager = restaurantUserManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
            //this.username = configuration["dbusername"];
        }

        //[AllowAnonymous]
        [HttpGet]
        public IActionResult AddResturant()
        {
            string actionName = "AddResturant";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var countryList = _DBContext.Country.OrderBy(r => r.CountryName).ToList().Select(rr =>
                            new SelectListItem
                            {
                                Value = rr.CountryName.ToString(),
                                Text = rr.CountryName
                            }).ToList();

                var districtList = _DBContext.District.OrderBy(r => r.DistrictName).ToList().Select(rr =>
                            new SelectListItem
                            {
                                Value = rr.DistrictName.ToString(),
                                Text = rr.DistrictName
                            }).ToList();

                var restaurantStatus = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Active", Text = "Active" },
                    new SelectListItem { Value = "Inactive", Text = "Inactive" }
                };

                ViewBag.RestaurantStatus = restaurantStatus;
                ViewBag.Countries = countryList;
                ViewBag.Districts = districtList;

                return View();
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View();
            }
        }

        //[AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddResturant(ResturantModel resturantModel)
        {
            string actionName = "AddResturant";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                string connString = this._configuration.GetConnectionString("3DPOSDBConnection");
                resturantModel.DBConnectionString = connString;
                var restaurantData = _resturantRepository.SaveResturant(resturantModel, loggedInUserId);

                var countryList = _DBContext.Country.OrderBy(r => r.CountryName).ToList().Select(rr =>
                            new SelectListItem
                            {
                                Value = rr.CountryName.ToString(),
                                Text = rr.CountryName
                            }).ToList();

                var districtList = _DBContext.District.OrderBy(r => r.DistrictName).ToList().Select(rr =>
                            new SelectListItem
                            {
                                Value = rr.DistrictName.ToString(),
                                Text = rr.DistrictName
                            }).ToList();

                var restaurantStatus = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Active", Text = "Active" },
                    new SelectListItem { Value = "Inactive", Text = "Inactive" }
                };

                if (restaurantData == null)
                {
                    ViewBag.Countries = countryList;
                    ViewBag.Districts = districtList;
                    ViewBag.RestaurantStatus = restaurantStatus;

                    return View(resturantModel);
                }
                else
                {
                    AuditLog auditLog = new AuditLog();

                    auditLog.AuditTime = DateTime.UtcNow;
                    auditLog.AreaName = _areaName;
                    auditLog.ControllerName = _controllerName;
                    auditLog.ActionName = actionName;
                    auditLog.AuditBy = auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    auditLog.PreviousInformation = null;
                    auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { restaurantData });

                    if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                    {
                        ViewBag.Countries = countryList;
                        ViewBag.Districts = districtList;
                        ViewBag.RestaurantStatus = restaurantStatus;

                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return View(resturantModel);
                    }
                    else
                    {
                        return RedirectToAction("ResturantList");
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View(resturantModel);
            }
        }

        //[AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> ResturantList(string SearchText = "", int pg = 1)
        {
            string actionName = "ResturantList";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<ResturantModel> resturantlist = new List<ResturantModel>();
            List<ResturantModel> emptyResturantlist = new List<ResturantModel>();
            int recsCount = 0;

            try
            {
                if (SearchText != "" && SearchText != null)
                {
                   // var resturantlistData = await _resturantRepository.GetResturantListBySearchValue(SearchText, loggedInUserId).Select(x=>x.ResturentCode).ToArrayAsync();
                    resturantlist = await _resturantRepository.GetResturantListBySearchValue(SearchText, loggedInUserId);
                }
                else
                {
                    resturantlist = await _resturantRepository.GetResturantList(loggedInUserId);
                }

                if (resturantlist != null)
                {
                    recsCount = resturantlist.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var districtData = resturantlist.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No Restaurant added yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(districtData);
                    }
                    else
                    {
                        const int pageSize = 5;
                        if (pg < 1)
                        {
                            pg = 1;
                        }

                        var search_Pager = new SearchBarAndPager(recsCount, pg, pageSize) { Controller = _controllerName, Action = actionName, SearchText = SearchText };

                        int recSkip = (pg - 1) * pageSize;

                        var countryData = resturantlist.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(countryData);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(emptyResturantlist);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View();
            }
        }
        [Authorize]
        [AcceptVerbs("Get", "Post")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRestautantDatabase(int id)
        {
            Database.ModelClass.LocalSQLDBCreation localSQLDBCreation = new Database.ModelClass.LocalSQLDBCreation(_configuration);
            LocalSQLDBCreationData localSQLDBCreationData = new LocalSQLDBCreationData();

            localSQLDBCreationData = localSQLDBCreation.StoreLocalSQLDBCreationData();

            string actionName = "AddRestautantDatabase";

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            ResturantModel resturant = await _resturantRepository.GetResturantInfoByID(id, loggedInUserId);

            SqlConnection sqlConnection = null;

            if (!string.IsNullOrEmpty(localSQLDBCreationData.DataSource))
                sqlConnection = new SqlConnection("Data Source=" + localSQLDBCreationData.DataSource + ";Initial Catalog=;User Id=" + localSQLDBCreationData.UserId + "; Password=" + localSQLDBCreationData.Password + "");
            else if (!string.IsNullOrEmpty(localSQLDBCreationData.Server))
                sqlConnection = new SqlConnection("Server=" + localSQLDBCreationData.Server + ";Initial Catalog=;User Id=" + localSQLDBCreationData.UserId + "; Password=" + localSQLDBCreationData.Password + "");

            try
            {
                using (sqlConnection)
                {
                    String connectionString;
                    connectionString = "CREATE DATABASE " + resturant.ResturentCode.ToString() + " ON PRIMARY " +
                   "(NAME = " + resturant.ResturentCode.ToString() + "_Data, " +
                   "FILENAME = 'C:\\Program Files\\Microsoft SQL Server\\MSSQL15.SQLEXPRESS01\\MSSQL\\DATA\\" + resturant.ResturentCode.ToString() + ".mdf', " +
                   "SIZE = 4MB, MAXSIZE = 10MB, FILEGROWTH = 10%)" +
                   "LOG ON (NAME = " + resturant.ResturentCode.ToString() + "_Log, " +
                   "FILENAME = 'C:\\Program Files\\Microsoft SQL Server\\MSSQL15.SQLEXPRESS01\\MSSQL\\DATA\\" + resturant.ResturentCode.ToString() + "Log.ldf', " +
                   "SIZE = 1MB, " +
                   "MAXSIZE = 5MB, " +
                   "FILEGROWTH = 10%)";

                    SqlCommand myCommand = new SqlCommand(connectionString, sqlConnection);

                    sqlConnection.Open();
                    myCommand.ExecuteNonQuery();

                    if (!string.IsNullOrEmpty(localSQLDBCreationData.DataSource))
                        resturant.DBConnectionString = "Data Source=" + localSQLDBCreationData.DataSource + ";Initial Catalog=" + resturant.ResturentCode.ToString() + ";User Id=" + localSQLDBCreationData.UserId + "; Password=" + localSQLDBCreationData.Password + "";

                    else if (!string.IsNullOrEmpty(localSQLDBCreationData.Server))
                        resturant.DBConnectionString = "Server=" + localSQLDBCreationData.Server + ";Initial Catalog=" + resturant.ResturentCode.ToString() + ";User Id=" + localSQLDBCreationData.UserId + "; Password=" + localSQLDBCreationData.Password + "";


                    resturant.IsTableCreated = false;

                    if (!_resturantRepository.AddDatabaseToRestaurant(resturant, loggedInUserId))
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.RestaurantDBNotAdded.ToString());
                        return RedirectToAction("ResturantList");
                    }
                    else
                    {
                        return RedirectToAction("ResturantList");
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.RestaurantDBNotAdded.ToString());
                return RedirectToAction("ResturantList");
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                {
                    sqlConnection.Close();
                }
            }
        }

        //[AcceptVerbs("Get", "Post")]
        ////[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddRestautantDatabaseTable(int id)
        //{
        //    string actionName = "AddRestautantDatabaseTable";

        //    var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

        //    ResturantModel resturant = await _resturantRepository.GetResturantInfoByID(id, loggedInUserId);

        //    try
        //    {
        //        var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>().UseSqlServer(resturant.DBConnectionString);
        //        //optionsBuilder.UseSqlServer(resturant.DBConnectionString);

        //        using (var context = new RestaurantDbContext(optionsBuilder.Options))
        //        {
        //            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        //            if (pendingMigrations.Any())
        //            {
        //                await context.Database.MigrateAsync();
        //            }
        //            var lastAppliedMigration = (await context.Database.GetAppliedMigrationsAsync()).Last();
        //        }

        //        bool result = _resturantRepository.CreateRestaurantUser(resturant.ResturentCode, loggedInUserId, resturant.DBConnectionString);

        //        if (result == true)
        //        {
        //            resturant.IsTableCreated = true;
        //            resturant.RestaurantUserId = resturant.ResturentCode;
        //            resturant.Email = resturant.ResturentCode + "@admin.com";
        //            resturant.Password = "asdf1234";

        //            if (!_resturantRepository.AddDatabaseToRestaurant(resturant, loggedInUserId))
        //            {
        //                ModelState.AddModelError(string.Empty, _responseMessage.RestaurantTableNotAdded.ToString());
        //                return RedirectToAction("ResturantList");
        //            }
        //            else
        //            {
        //                return RedirectToAction("ResturantList");
        //            }
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(string.Empty, _responseMessage.RestaurantUserNotAdded.ToString());
        //            return RedirectToAction("ResturantList");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //        ModelState.AddModelError(string.Empty, _responseMessage.RestaurantTableNotAdded.ToString());
        //        return RedirectToAction("ResturantList");
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> RestaurantActionsView(int id)
        {
            string actionName = "RestaurantActionsView";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                ResturantModel resturant = await _resturantRepository.GetResturantInfoByID(id, loggedInUserId);

                ViewBag.RestaurantId = id;
                ViewBag.RestaurantName = resturant.RestaurantName;
                return View();
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.RestaurantTableNotAdded.ToString());
                return RedirectToAction("ResturantList");
            }
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> RestaurantUserList(string SearchText = "", int pg = 1, int id = 0)
        {
            string actionName = "RestaurantUserList";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<RestaurantUsers> restaurantUserslist = new List<RestaurantUsers>();
            List<RestaurantUsers> emptyResturantUserlist = new List<RestaurantUsers>();
            int recsCount = 0;

            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    restaurantUserslist = await _resturantRepository.RestaurantUserListBySearchValue(SearchText, loggedInUserId, id);
                }
                else
                {
                    restaurantUserslist = await _resturantRepository.RestaurantUserList(loggedInUserId, id);
                }

                if (restaurantUserslist != null)
                {
                    recsCount = restaurantUserslist.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var restaurantUserData = restaurantUserslist.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No Restaurant user added yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;
                        ViewBag.RestaurantId = id;
                        return View(restaurantUserData);
                    }
                    else
                    {
                        const int pageSize = 5;
                        if (pg < 1)
                        {
                            pg = 1;
                        }

                        var search_Pager = new SearchBarAndPager(recsCount, pg, pageSize) { Controller = _controllerName, Action = actionName, SearchText = SearchText };

                        int recSkip = (pg - 1) * pageSize;

                        var restaurantUserData = restaurantUserslist.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;
                        ViewBag.RestaurantId = id;
                        return View(restaurantUserData);
                    }
                }
                else
                {
                    ViewBag.RestaurantId = id;
                    ModelState.AddModelError(string.Empty, _responseMessage.EmptyRestaurantUser.ToString());
                    return View(emptyResturantUserlist);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View();
            }
        }


        //[AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> AddRestaurantUser(int id)
        {
            string actionName = "AddRestaurantUser";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<RestaurantUserRole> restaurantUserRoles = new List<RestaurantUserRole>();
            try
            {
                restaurantUserRoles =await _resturantRepository.GetRestaurantUserRoles(loggedInUserId, id);

                //var databaseList = _DBContext.DBCatalog.Where(x=>x.IsDBCreated==true).OrderBy(r => r.ID).ToList().Select(rr =>
                //            new SelectListItem
                //            {
                //                Value = rr.ID.ToString(),
                //                Text = rr.DatabaseName
                //            }).ToList();

                var userRoles = restaurantUserRoles.OrderBy(r => r.RoleId).ToList().Select(rr =>
                                new SelectListItem
                                {
                                    Value = rr.RoleId.ToString(),
                                    Text = rr.RoleName
                                }).ToList();

                ViewBag.RestaurantId = id;
                ViewBag.UserRoles = userRoles;

                return View();
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View();
            }
        }

        //[AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRestaurantUser(RestaurantUserWithDatabase restaurantUserWithDatabase)
        {
            string actionName = "AddRestaurantUser";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<RestaurantUserRole> restaurantUserRoles = new List<RestaurantUserRole>();

            try
            {
                var userRoles = restaurantUserRoles.OrderBy(r => r.RoleId).ToList().Select(rr =>
                                new SelectListItem
                                {
                                    Value = rr.RoleId.ToString(),
                                    Text = rr.RoleName
                                }).ToList();

                var isExistUser = await _resturantRepository.CheckExistRestaurantUser(restaurantUserWithDatabase, loggedInUserId);
                if (isExistUser == "True")
                {
                    ViewBag.RestaurantId = restaurantUserWithDatabase.RestaurantId;
                    ViewBag.UserRoles = userRoles;
                    ModelState.AddModelError(string.Empty, _responseMessage.UsernameExist.ToString());
                    return View(restaurantUserWithDatabase);
                }
                else if(isExistUser == "False")
                {
                    restaurantUserRoles = await _resturantRepository.GetRestaurantUserRoles(loggedInUserId, restaurantUserWithDatabase.RestaurantId);
                    var addNewUserData = _resturantRepository.SaveNewRestaurantUser(restaurantUserWithDatabase, loggedInUserId);

                    if (addNewUserData == null)
                    {
                        ViewBag.RestaurantId = restaurantUserWithDatabase.RestaurantId;
                        ViewBag.UserRoles = userRoles;
                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return View(restaurantUserWithDatabase);
                    }
                    else
                    {
                        AuditLog auditLog = new AuditLog();

                        auditLog.AuditTime = DateTime.UtcNow;
                        auditLog.AreaName = _areaName;
                        auditLog.ControllerName = _controllerName;
                        auditLog.ActionName = actionName;
                        auditLog.AuditBy = auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        auditLog.PreviousInformation = null;
                        auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { restaurantUserWithDatabase });

                        if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                        {
                            ViewBag.RestaurantId = restaurantUserWithDatabase.RestaurantId;
                            ViewBag.UserRoles = userRoles;

                            ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                            return View(restaurantUserWithDatabase);
                        }
                        else
                        {
                            return RedirectToAction("RestaurantUserList", new { id = restaurantUserWithDatabase.RestaurantId });
                        }
                    }
                }
                else
                {
                    ViewBag.RestaurantId = restaurantUserWithDatabase.RestaurantId;
                    ViewBag.UserRoles = userRoles;
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(restaurantUserWithDatabase);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View(restaurantUserWithDatabase);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditRestaurant(int id)
        {
            string actionName = "EditRestaurant";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                ResturantModel resturant = await _resturantRepository.GetResturantInfoByID(id, loggedInUserId);

                var countryList = _DBContext.Country.OrderBy(r => r.CountryName).ToList().Select(rr =>
                             new SelectListItem
                             {
                                 Value = rr.CountryName.ToString(),
                                 Text = rr.CountryName
                             }).ToList();

                var districtList = _DBContext.District.OrderBy(r => r.DistrictName).ToList().Select(rr =>
                            new SelectListItem
                            {
                                Value = rr.DistrictName.ToString(),
                                Text = rr.DistrictName
                            }).ToList();

                var restaurantStatus = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Active", Text = "Active" },
                    new SelectListItem { Value = "Inactive", Text = "Inactive" }
                };

                if (resturant.DatabaseName == null)
                {
                    var dbCatalogList = _DBContext.DBCatalog.OrderBy(r => r.DatabaseName).Where(x => x.CanInsertRestaurant == true && x.IsDBCreated == true).ToList().Select(rr =>
                                  new SelectListItem
                                  {
                                      Value = rr.ID.ToString(),
                                      Text = rr.DatabaseName
                                  }).ToList();

                    ViewBag.Countries = countryList;
                    ViewBag.Districts = districtList;
                    ViewBag.DBCatalogs = dbCatalogList;
                    ViewBag.RestaurantStatus = restaurantStatus;

                    if (resturant == null)
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return View();
                    }
                    else
                    {
                        return View(resturant);
                    }
                }
                else
                {
                    ViewBag.Countries = countryList;
                    ViewBag.Districts = districtList;
                    ViewBag.RestaurantStatus = restaurantStatus;

                    if (resturant == null)
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return View();
                    }
                    else
                    {
                        return View(resturant);
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }
        }

        //[AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRestaurant(ResturantModel resturantModel)
        {
            string actionName = "EditRestaurant";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var countryList = _DBContext.Country.OrderBy(r => r.CountryName).ToList().Select(rr =>
                             new SelectListItem
                             {
                                 Value = rr.CountryName.ToString(),
                                 Text = rr.CountryName
                             }).ToList();

                var districtList = _DBContext.District.OrderBy(r => r.DistrictName).ToList().Select(rr =>
                            new SelectListItem
                            {
                                Value = rr.DistrictName.ToString(),
                                Text = rr.DistrictName
                            }).ToList();

                var restaurantStatus = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Active", Text = "Active" },
                    new SelectListItem { Value = "Inactive", Text = "Inactive" }
                };

                var dbCatalogList = _DBContext.DBCatalog.OrderBy(r => r.DatabaseName).Where(x => x.CanInsertRestaurant == true && x.IsDBCreated == true).ToList().Select(rr =>
                                  new SelectListItem
                                  {
                                      Value = rr.ID.ToString(),
                                      Text = rr.DatabaseName
                                  }).ToList();

                var checkCanInsertRestaurantInDBCatalog = _DBContext.DBCatalog.Where(x => x.ID == resturantModel.DBCatalogID).FirstOrDefault();

                Resturant UpdateRestaurantData = new Resturant();
                ResturantModel previousesturantData = await _resturantRepository.GetResturantInfoByID(resturantModel.ResturantID, loggedInUserId);

                if (previousesturantData.DatabaseName == null && resturantModel.DBCatalogID != 0 && checkCanInsertRestaurantInDBCatalog.CanInsertRestaurant == true)
                {
                    UpdateRestaurantData = await _resturantRepository.CreateRestaurantAndRestaurantUserInDBCatalogDatabase(resturantModel, loggedInUserId);

                    if (UpdateRestaurantData != null)
                    {
                        AuditLog auditLog = new AuditLog();

                        auditLog.AuditTime = DateTime.UtcNow;
                        auditLog.AreaName = _areaName;
                        auditLog.ControllerName = _controllerName;
                        auditLog.ActionName = actionName;
                        auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        auditLog.PreviousInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { previousesturantData }); ;
                        auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { UpdateRestaurantData });

                        if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                        {
                            ViewBag.Countries = countryList;
                            ViewBag.Districts = districtList;
                            ViewBag.DBCatalogs = dbCatalogList;
                            ViewBag.RestaurantStatus = restaurantStatus;

                            ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                            return View(resturantModel);
                        }
                        else
                        {
                            return RedirectToAction("ResturantList");
                        }
                    }
                    else
                    {
                        ViewBag.Countries = countryList;
                        ViewBag.Districts = districtList;
                        ViewBag.DBCatalogs = dbCatalogList;
                        ViewBag.RestaurantStatus = restaurantStatus;

                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return View(resturantModel);
                    }
                }
                else
                {
                    UpdateRestaurantData = await _resturantRepository.UpdateRestaurantInformation(resturantModel, loggedInUserId);

                    if (UpdateRestaurantData != null)
                    {
                        AuditLog auditLog = new AuditLog();

                        auditLog.AuditTime = DateTime.UtcNow;
                        auditLog.AreaName = _areaName;
                        auditLog.ControllerName = _controllerName;
                        auditLog.ActionName = actionName;
                        auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        auditLog.PreviousInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { previousesturantData }); ;
                        auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { UpdateRestaurantData });

                        if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                        {
                            ViewBag.Countries = countryList;
                            ViewBag.Districts = districtList;
                            ViewBag.RestaurantStatus = restaurantStatus;

                            ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                            return View(resturantModel);
                        }
                        else
                        {
                            return RedirectToAction("ResturantList");
                        }
                    }
                    else
                    {
                        ViewBag.Countries = countryList;
                        ViewBag.Districts = districtList;
                        ViewBag.RestaurantStatus = restaurantStatus;

                        ModelState.AddModelError(string.Empty, _responseMessage.RestaurantInformationNotUpdated.ToString());
                        return View(resturantModel);
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View(resturantModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            string actionName = "DeleteRestaurant";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            DBCatalog _DBCatalog = new DBCatalog();

            try
            {
                ResturantModel resturant = await _resturantRepository.GetResturantInfoByID(id, loggedInUserId);

                if (resturant.DatabaseName != null)
                {
                    var result = await _resturantRepository.RemoveRestaurant(id, loggedInUserId);

                    if (result == false)
                    {
                        var jsonResult = Json(new Confirmation { output = "error", msg = "This restaurant didn't delete! Please check Error Log or Contact with your support engineer!" });
                        return jsonResult;
                    }
                    else
                    {
                        var jsonResult = Json(new Confirmation { output = "success", msg = "Restaurant deleted successfully!" });
                        return jsonResult;
                    }
                }
                else
                {
                    return Json(new Confirmation { output = "error", msg = "You can't delete this restaurant. A database was created for this restairant." });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return Json(new Confirmation { output = "error", msg = "Please check Error Log or Contact with your support engineer!" });
            }
        }

    }
}
