using _3DPOSRegistrationApp.Database;
using _3DPOSRegistrationApp.Database.Model;
using CommonEntityModel.EntityModel;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CommonEntityModel.ModelClass;

namespace _3DPOSRegistrationApp.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class DBCatalogController : Controller
    {
        private const string _areaName = "3DPOSRegistration Module";
        private const string _controllerName = "DBCatalog";
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

        public DBCatalogController(_3DPOS_DBContext DBContext, RestaurantDbContext restaurantDbContext, IUserRepository userRepository, IErrorLogRepository errorLogRepository, IAuditLogRepository auditLogRepository, IRoleLogRepository roleLogRepository, IStatusLogRepository statusLogRepository, ICommonRepository commonRepository, IResturantRepository resturantRepository, IDBCatalogRepository DBCatalogRepository, IConfiguration configuration, UserManager<ApplicationUser> userManager, UserManager<AspNetUsers> restaurantUserManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
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

        [AcceptVerbs("Get", "Post")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTableToRestautantDatabase(int id)
        {
            string actionName = "AddTableToRestautantDatabase";

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var dbCatalogData = await _DBCatalogRepository.GetDBCatalogByID(id, loggedInUserId);

            try
            {
                var migrationFileName = "";
                var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>().UseSqlServer(dbCatalogData.ConnectionString);

                using (var context = new RestaurantDbContext(optionsBuilder.Options))
                {
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

                    if (pendingMigrations.Any())
                    {
                        await context.Database.MigrateAsync();
                    }
                    var lastAppliedMigration = (await context.Database.GetAppliedMigrationsAsync()).Last();
                    migrationFileName = lastAppliedMigration;
                }

                var IsMigrationFileAdded = _DBCatalogRepository.AddLastMigrationFileInDBCatalog(migrationFileName, id, loggedInUserId);

                if (_DBCatalogRepository.AddDatabaseAndTable(id, loggedInUserId))
                {
                    var jsonResult = Json(new Confirmation { output = "success", msg = "Table added successfully!" });
                    return jsonResult;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.RestaurantTableNotAdded.ToString());
                    return Json(new Confirmation { output = "error", msg = "Table didn't add!" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.RestaurantTableNotAdded.ToString());
                return Json(new Confirmation { output = "error", msg = "Table didn't add!" });
            }
        }

        //[AllowAnonymous]
        [HttpGet]
        public IActionResult AddDBCatalog()
        {
            string actionName = "AddDBCatalog";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var canInsertRestaurant = new List<SelectListItem>
                {
                    new SelectListItem { Value = "True", Text = "True" },
                    new SelectListItem { Value = "False", Text = "False" }
                };

                ViewBag.CanAddRestaurant = canInsertRestaurant;

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
        public IActionResult AddDBCatalog(DBCatalogViewModel _DBCatalogViewModel)
        {
            string actionName = "AddDBCatalog";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var dbCatalogData = _DBCatalogRepository.SaveDBCatalog(_DBCatalogViewModel, loggedInUserId);

                var canInsertRestaurant = new List<SelectListItem>
                        {
                            new SelectListItem { Value = "True", Text = "True" },
                            new SelectListItem { Value = "False", Text = "False" }
                        };

                if (dbCatalogData == null)
                {
                    ViewBag.CanAddRestaurant = canInsertRestaurant;
                    ModelState.AddModelError(string.Empty, _responseMessage.DBCatalogExistOrTryCatchError.ToString());
                    return View(_DBCatalogViewModel);
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
                    auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { dbCatalogData });

                    if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                    {
                        ViewBag.CanAddRestaurant = canInsertRestaurant;

                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return View(_DBCatalogViewModel);
                    }
                    else
                    {
                        return RedirectToAction("ResturantDBCatalogList");
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View(_DBCatalogViewModel);
            }
        }

        //[AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> ResturantDBCatalogList(string SearchText = "", int pg = 1)
        {
            string actionName = "ResturantDBCatalogList";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<DBCatalogViewModel> DBCatalogList = new List<DBCatalogViewModel>();
            List<DBCatalogViewModel> emptyDBCatalogList = new List<DBCatalogViewModel>();
            int recsCount = 0;

            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    DBCatalogList = await _DBCatalogRepository.GetDBCatalogListBySearchValue(SearchText, loggedInUserId);
                }
                else
                {
                    DBCatalogList = await _DBCatalogRepository.GetDBCatalogList(loggedInUserId);
                }

                if (DBCatalogList != null)
                {
                    recsCount = DBCatalogList.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var DBCatalogData = DBCatalogList.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No DBCatalog added yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(DBCatalogData);
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

                        var DBCatalogData = DBCatalogList.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(DBCatalogData);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(emptyDBCatalogList);
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
        public async Task<IActionResult> EditDBCatalog(int id)
        {
            string actionName = "EditDBCatalog";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            DBCatalog _DBCatalog = new DBCatalog();

            try
            {
                var canInsertRestaurant = new List<SelectListItem>
                {
                    new SelectListItem { Value = "True", Text = "True" },
                    new SelectListItem { Value = "False", Text = "False" }
                };

                ViewBag.CanAddRestaurant = canInsertRestaurant;

                var dbCatalogData = await _DBCatalogRepository.GetDBCatalogByID(id, loggedInUserId);

                if (dbCatalogData.IsTryCatchError == true)
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(_DBCatalog);
                }
                else
                {
                    return View(dbCatalogData);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDBCatalog(DBCatalogViewModel dbCatalogViewModel)
        {
            string actionName = "EditDBCatalog";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var dbCatalogData = await _DBCatalogRepository.GetDBCatalogByID(dbCatalogViewModel.ID, loggedInUserId);
                var result = _DBCatalogRepository.EditDBCatalog(dbCatalogViewModel, loggedInUserId);

                if (result != null)
                {
                    AuditLog auditLog = new AuditLog();

                    auditLog.AuditTime = DateTime.UtcNow;
                    auditLog.AreaName = _areaName;
                    auditLog.ControllerName = _controllerName;
                    auditLog.ActionName = actionName;
                    auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    auditLog.PreviousInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { dbCatalogData }); ;
                    auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { result });

                    if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return View(dbCatalogViewModel);
                    }
                    else
                    {
                        return RedirectToAction("ResturantDBCatalogList");
                    }
                }
                return View(dbCatalogViewModel);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View(dbCatalogViewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> RemoveDBCatalog(int id)
        {
            string actionName = "RemoveDBCatalog";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            try
            {
                var dbCatalogData = await _DBCatalogRepository.RemoveDBCatalog(id, loggedInUserId);

                if (dbCatalogData == "false")
                {
                    var jsonResult = Json(new Confirmation { output = "error", msg = "This DBCatalog can't delete! Already there are some restaurent in this database!" });
                    return jsonResult;
                }
                else if (dbCatalogData == "true")
                {
                    var jsonResult = Json(new Confirmation { output = "success", msg = "DBCatalog remove successfully!" });
                    return jsonResult;
                }
                else
                {
                    return Json(new Confirmation { output = "error", msg = "Please check Error Log or Contact with your support engineer!" });
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

