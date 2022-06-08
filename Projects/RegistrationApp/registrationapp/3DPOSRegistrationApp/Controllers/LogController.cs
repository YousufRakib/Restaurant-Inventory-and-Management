using _3DPOSRegistrationApp.Database;
using _3DPOSRegistrationApp.Repository.IRepository.ILogRepository;
using _3DPOSRegistrationApp.Utility.LogViewModel;
using _3DPOSRegistrationApp.Utility.ServiceModel;
using _3DPOSRegistrationApp.Utility.UserRoleModel;
using _3DPOSRegistrationApp.Utility.UserStatus;
using CommonEntityModel.ModelClass;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class LogController : Controller
    {
        private const string _areaName = "3DPOSRegistration Module";
        private const string _controllerName = "Log";
        private readonly _3DPOS_DBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IRoleLogRepository _roleLogRepository;
        private readonly IStatusLogRepository _statusLogRepository;
        private readonly ResponseMessage _responseMessage = new ResponseMessage();

        public LogController(_3DPOS_DBContext DBContext, IErrorLogRepository errorLogRepository, IAuditLogRepository auditLogRepository, IRoleLogRepository roleLogRepository, IStatusLogRepository statusLogRepository)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._auditLogRepository = auditLogRepository;
            this._roleLogRepository = roleLogRepository;
            this._statusLogRepository = statusLogRepository;
        }

        //[AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> GetErrorLogList(string SearchText = "", int pg = 1)
        {
            string actionName = "GetErrorLogList";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            int recsCount = 0;

            List<CommonInfo> errorloglist = new List<CommonInfo>();
            List<CommonInfo> emptyErrorloglist = new List<CommonInfo>();

            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    errorloglist = await _errorLogRepository.GetErrorLogListBySearchValue(SearchText, loggedInUserId);
                }
                else
                {
                    errorloglist = await _errorLogRepository.GetErrorLogList(loggedInUserId);
                }

                if (errorloglist != null)
                {
                    recsCount = errorloglist.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var userData = errorloglist.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No update found!";
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(userData);
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

                        var userData = errorloglist.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(userData);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(emptyErrorloglist);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View(errorloglist);
            }
        }

        //[AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> GetAuditLogList(string AuditByTxt = "", string ControllerNameTxt = "", string ActionNameTxt = "", string PreInfoTxt = "", string UpdatedInfoNameTxt = "", int pg = 1)
        {
            string actionName = "GetAuditLogList";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            int recsCount = 0;

            List<AuditLogViewModel> auditloglist = new List<AuditLogViewModel>();
            List<AuditLogViewModel> emptyAuditloglist = new List<AuditLogViewModel>();

            try
            {
                if ((ControllerNameTxt != "" && ControllerNameTxt != null) || (ActionNameTxt != "" && ActionNameTxt != null) || (PreInfoTxt != "" && PreInfoTxt != null) || (UpdatedInfoNameTxt != "" && UpdatedInfoNameTxt != null))
                {
                    auditloglist = await _auditLogRepository.GetAuditLogListBySearchValue(AuditByTxt, ControllerNameTxt, ActionNameTxt, PreInfoTxt, UpdatedInfoNameTxt, loggedInUserId);
                }
                else
                {
                    auditloglist = await _auditLogRepository.GetAuditLogList(loggedInUserId);
                }

                if (auditloglist != null)
                {
                    recsCount = auditloglist.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPagerForAuditLog(1, 1, 1) { Controller = _controllerName, Action = actionName, ControllerNameTxt = ControllerNameTxt, ActionNameTxt = ActionNameTxt, PreInfoTxt = PreInfoTxt, UpdatedInfoNameTxt = UpdatedInfoNameTxt };
                        var userData = auditloglist.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No update found!";
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(userData);
                    }
                    else
                    {
                        const int pageSize = 5;
                        if (pg < 1)
                        {
                            pg = 1;
                        }

                        var search_Pager = new SearchBarAndPagerForAuditLog(recsCount, pg, pageSize) { Controller = _controllerName, Action = actionName, ControllerNameTxt = ControllerNameTxt, ActionNameTxt = ActionNameTxt, PreInfoTxt = PreInfoTxt, UpdatedInfoNameTxt = UpdatedInfoNameTxt };

                        int recSkip = (pg - 1) * pageSize;

                        var userData = auditloglist.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(userData);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(emptyAuditloglist);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View(auditloglist);
            }
        }

        //[AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ErrorLogDetails(int id)
        {
            string actionName = "ErrorLogDetails";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var errorDetails = await _errorLogRepository.GetErrorDetails(id, loggedInUserId);

                if (errorDetails.IsTrue == false)
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View();
                }
                else if (errorDetails != null)
                {
                    return View(errorDetails);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.EmptyErrorDetails.ToString());
                    return View();
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
        public async Task<IActionResult> UserRoleUpdateLogList(string SearchText = "", int pg = 1)
        {
            string actionName = "UserRoleUpdateLogList";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            int recsCount = 0;
            List<UserRoleUpdateLogModel> roleUpdateLogList = new List<UserRoleUpdateLogModel>();
            List<UserRoleUpdateLogModel> emptyRoleUpdateLogList = new List<UserRoleUpdateLogModel>();

            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    roleUpdateLogList = await _roleLogRepository.GetUserRoleLogDataBySearchValue(SearchText, loggedInUserId);
                }
                else
                {
                    roleUpdateLogList = await _roleLogRepository.GetUserRoleLogList(loggedInUserId);
                }

                if (roleUpdateLogList != null)
                {
                    recsCount = roleUpdateLogList.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var userData = roleUpdateLogList.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No update found!";
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(userData);

                        //ViewBag.Message = "No users found!";
                        //return View(roleUpdateLogList);
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

                        var userData = roleUpdateLogList.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.searchBar_Pager = search_Pager;
                        return View(userData);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(emptyRoleUpdateLogList);
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
        public async Task<IActionResult> UserStatusUpdateLogList(string SearchText = "", int pg = 1)
        {
            string actionName = "UserStatusUpdateLogList";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            int recsCount = 0;
            List<UserStatusUpdateLogModel> statusUpdateLogList = new List<UserStatusUpdateLogModel>();
            List<UserStatusUpdateLogModel> emptyStatusUpdateLogList = new List<UserStatusUpdateLogModel>();

            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    statusUpdateLogList = await _statusLogRepository.GetUserStatusLogDataBySearchValue(SearchText, loggedInUserId);
                }
                else
                {
                    statusUpdateLogList = await _statusLogRepository.GetUserStatusLogList(loggedInUserId);
                }

                if (statusUpdateLogList != null)
                {
                    recsCount = statusUpdateLogList.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var userData = statusUpdateLogList.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No update found!";
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(userData);
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

                        var userData = statusUpdateLogList.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.searchBar_Pager = search_Pager;
                        return View(userData);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(emptyStatusUpdateLogList);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View();
            }
        }

    }
}
