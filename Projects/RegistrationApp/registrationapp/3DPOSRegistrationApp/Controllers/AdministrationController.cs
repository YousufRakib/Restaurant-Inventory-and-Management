using CommonEntityModel.EntityModel;
using _3DPOSRegistrationApp.Repository.IRepository.ILogRepository;
using _3DPOSRegistrationApp.Repository.IRepository.IUserRepository;
using _3DPOSRegistrationApp.Utility.ServiceModel;
using _3DPOSRegistrationApp.Utility.UserRoleModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CommonEntityModel.ModelClass;

namespace _3DPOSRegistrationApp.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdministrationController : Controller
    {
        private const string _areaName = "3DPOSRegistration Module";
        private const string _controllerName = "Administration";
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly CommonInfo _commonInfo = new CommonInfo();
        private readonly ResponseMessage _responseMessage = new ResponseMessage();
        public AdministrationController(RoleManager<ApplicationRole> _roleManager, IErrorLogRepository errorLogRepository, IUserRepository userRepository, IAuditLogRepository auditLogRepository)
        {
            this._roleManager = _roleManager;
            this._errorLogRepository = errorLogRepository;
            this._userRepository = userRepository;
            this._auditLogRepository = auditLogRepository;
        }

        //[Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        //[Authorize(Roles ="SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(RoleViewModel roleViewModel)
        {
            string actionName = "CreateRole";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var roleExists = await _roleManager.FindByNameAsync(roleViewModel.RoleName);
                if (roleExists != null)
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.RoleExist.ToString());
                    return View(roleViewModel);
                }

                ApplicationRole identityRole = new ApplicationRole
                {
                    Name = roleViewModel.RoleName
                };

                IdentityResult result = await _roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    AuditLog auditLog = new AuditLog();

                    auditLog.AuditTime = DateTime.UtcNow;
                    auditLog.AreaName = _areaName;
                    auditLog.ControllerName = _controllerName;
                    auditLog.ActionName = actionName;
                    auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    auditLog.PreviousInformation = null;
                    auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { identityRole });

                    if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return View(roleViewModel);
                    }
                    else
                    {
                        //await signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Home", "User");
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                ModelState.AddModelError(string.Empty, _responseMessage.WrongRoleInfo.ToString());
                return View(roleViewModel);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View(roleViewModel);
            }
        }

        //[AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsRoleInUse(string roleName)
        {
            string actionName = "IsRoleInUse";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var user = await _roleManager.FindByNameAsync(roleName);

                if (user == null)
                {
                    return Json(true);
                }
                else
                {
                    return Json(_responseMessage.RoleExist.ToString());
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
        [HttpGet]
        public IActionResult RoleList()
        {
            string actionName = "RoleList";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var roles = _roleManager.Roles;
                return View(roles);
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
