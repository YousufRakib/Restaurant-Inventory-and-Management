using _3DPOSRegistrationApp.Database;
using _3DPOSRegistrationApp.Database.Model;
using CommonEntityModel.EntityModel;
using _3DPOSRegistrationApp.Repository.IRepository.ICommonRepository;
using _3DPOSRegistrationApp.Repository.IRepository.ILogRepository;
using _3DPOSRegistrationApp.Repository.IRepository.IResturantRepository;
using _3DPOSRegistrationApp.Repository.IRepository.IUserRepository;
using _3DPOSRegistrationApp.Utility.CommonModel;
using _3DPOSRegistrationApp.Utility.ServiceModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class CommonController : Controller
    {
        private const string _areaName = "3DPOSRegistration Module";
        private const string _controllerName = "Common";
        private readonly _3DPOS_DBContext _DBContext;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ResponseMessage _responseMessage = new ResponseMessage();
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IRoleLogRepository _roleLogRepository;
        private readonly IStatusLogRepository _statusLogRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly IResturantRepository _resturantRepository;
        private readonly ReturnTypeModel ReturnTypeModel = new ReturnTypeModel();


        public CommonController(_3DPOS_DBContext DBContext, IUserRepository userRepository, IErrorLogRepository errorLogRepository, IAuditLogRepository auditLogRepository, IRoleLogRepository roleLogRepository, IStatusLogRepository statusLogRepository, ICommonRepository commonRepository, IResturantRepository resturantRepository, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            this._DBContext = DBContext;
            this._userRepository = userRepository;
            this._errorLogRepository = errorLogRepository;
            this._auditLogRepository = auditLogRepository;
            this._roleLogRepository = roleLogRepository;
            this._statusLogRepository = statusLogRepository;
            this._commonRepository = commonRepository;
            this._resturantRepository = resturantRepository;
            this._configuration = configuration;
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
        }

        //[AllowAnonymous]
        [HttpGet]
        public IActionResult AddCountry()
        {
            return View();
        }

        //[AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCountry(CountryModel country)
        {
            string actionName = "AddCountry";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            CountryModel countryModel = new CountryModel();

            try
            {
                var countryData = _commonRepository.SaveCountry(country, loggedInUserId);

                if (countryData != null)
                {
                    AuditLog auditLog = new AuditLog();

                    auditLog.AuditTime = DateTime.UtcNow;
                    auditLog.AreaName = _areaName;
                    auditLog.ControllerName = _controllerName;
                    auditLog.ActionName = actionName;
                    auditLog.AuditBy = auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    auditLog.PreviousInformation = null;
                    auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { countryData });

                    if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return View(country);
                    }
                    else
                    {
                        return RedirectToAction("CountryList");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(country);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View(country);
            }
        }

        //[AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CountryList(string SearchText = "", int pg = 1)
        {
            string actionName = "CountryList";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<Country> countrylist = new List<Country>();
            List<Country> emptyCountrylist = new List<Country>();
            int recsCount = 0;
            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    countrylist = await _commonRepository.GetCountryListBySearchValue(SearchText, loggedInUserId);
                }
                else
                {
                    countrylist = await _commonRepository.GetCountryList(loggedInUserId);
                }

                if (countrylist != null)
                {
                    recsCount = countrylist.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var countryData = countrylist.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No country added yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(countryData);
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

                        var countryData = countrylist.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(countryData);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(emptyCountrylist);
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
        public IActionResult AddDistrict()
        {
            string actionName = "AddDistrict";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var countryList = _DBContext.Country.OrderBy(r => r.CountryName).ToList().Select(rr =>
                            new SelectListItem
                            {
                                Value = rr.CountryID.ToString(),
                                Text = rr.CountryName
                            }).ToList();

                ViewBag.Countries = countryList;
                return View();
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
        public IActionResult AddDistrict(DistrictModel district)
        {
            string actionName = "AddDistrict";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var countryList = _DBContext.Country.OrderBy(r => r.CountryName).ToList().Select(rr =>
                            new SelectListItem
                            {
                                Value = rr.CountryID.ToString(),
                                Text = rr.CountryName
                            }).ToList();

                var districtData = _commonRepository.SaveDistrict(district, loggedInUserId);

                if (districtData == null)
                {
                    ViewBag.Countries = countryList;

                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(district);

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
                    auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { districtData });

                    if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return View(district);
                    }
                    else
                    {
                        return RedirectToAction("DistrictList");
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View(district);
            }
        }

        //[AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> DistrictList(string SearchText = "", int pg = 1)
        {
            string actionName = "DistrictList";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<DistrictModel> districtlist = new List<DistrictModel>();
            List<DistrictModel> emptyDistrictlist = new List<DistrictModel>();
            int recsCount = 0;
            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    districtlist = await _commonRepository.GetDistrictListBySearchValue(SearchText, loggedInUserId);
                }
                else
                {
                    districtlist = await _commonRepository.GetDistrictList(loggedInUserId);
                }

                if (districtlist != null)
                {
                    recsCount = districtlist.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var districtData = districtlist.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No District added yet!";
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

                        var countryData = districtlist.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(countryData);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(emptyDistrictlist);
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
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsCountryInUse(string CountryName)
        {

            string actionName = "IsCountryInUse";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var user = _commonRepository.CheckExistCountry(CountryName, loggedInUserId);

                if (user == ReturnTypeModel.True)
                {
                    return Json(true);
                }
                else
                {
                    return Json(_responseMessage.CountryNameExist.ToString());
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(ex.Message);
            }
        }

        //[AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsDistrictInUse(string DistrictName)
        {

            string actionName = "IsDistrictInUse";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var user = _commonRepository.CheckExistCountry(DistrictName, loggedInUserId);

                if (user == ReturnTypeModel.True)
                {
                    return Json(true);
                }
                else
                {
                    return Json(_responseMessage.CountryNameExist.ToString());
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(ex.Message);
            }
        }
    }
}
