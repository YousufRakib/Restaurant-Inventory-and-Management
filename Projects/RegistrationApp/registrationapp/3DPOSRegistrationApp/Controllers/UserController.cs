using _3DPOSRegistrationApp.Database;
using _3DPOSRegistrationApp.Database.Model;
using CommonEntityModel.EntityModel;
using _3DPOSRegistrationApp.Repository.IRepository.ILogRepository;
using _3DPOSRegistrationApp.Repository.IRepository.IUserRepository;
using _3DPOSRegistrationApp.Utility.ServiceModel;
using _3DPOSRegistrationApp.Utility.UserModel_View;
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

    public class UserController : Controller
    {
        private const string _areaName = "3DPOSRegistration Module";
        private const string _controllerName = "User";
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


        public UserController(_3DPOS_DBContext DBContext, IUserRepository userRepository, IErrorLogRepository errorLogRepository, IAuditLogRepository auditLogRepository, IRoleLogRepository roleLogRepository, IStatusLogRepository statusLogRepository, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            this._DBContext = DBContext;
            this._userRepository = userRepository;
            this._errorLogRepository = errorLogRepository;
            this._auditLogRepository = auditLogRepository;
            this._roleLogRepository = roleLogRepository;
            this._statusLogRepository = statusLogRepository;
            this._configuration = configuration;
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
        }

        //[AllowAnonymous]
        [HttpGet]
        public IActionResult Home()
        {
            return View();
        }

        //[AllowAnonymous]
        [HttpGet]
        public IActionResult Registration()
        {
            //ViewData["roles"] = _roleManager.Roles.ToList();
            return View();
        }

        //[AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registration(RegistrationViewModel userModel)
        {
            string actionName = "Registration";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                //Existing User Check By Email
                var userExists = await _userManager.FindByEmailAsync(userModel.Email);
                if (userExists != null)
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.EmailExist.ToString());
                    return View(userModel);
                }

                var user = new ApplicationUser
                {
                    FullName = userModel.FullName,
                    UserName = userModel.Email,
                    NormalizedUserName = userModel.Email,
                    Email = userModel.Email,
                    NormalizedEmail = userModel.Email,
                    EmailConfirmed = true,
                    CreatedDate = DateTime.UtcNow,
                    UserStatus = (int)Utility.UserStatus.UserStatus.StatusTypes.Active,
                    DefaultPassword = userModel.Password
                };
                

                var result = await _userManager.CreateAsync(user, userModel.Password);
                //string RestaurantCode = "RestaurantCode";
                //var claim = new Claim(RestaurantCode, user.RestaurantCode);
                //var result2 = await _userManager.AddClaimAsync(user, claim);
                await _userManager.AddToRoleAsync(user, "User");

                if (result.Succeeded)
                {
                    var userInfo = await _userManager.FindByEmailAsync(user.Email);

                    AuditLog auditLog = new AuditLog();

                    auditLog.AuditTime = DateTime.UtcNow;
                    auditLog.AreaName = _areaName;
                    auditLog.ControllerName = _controllerName;
                    auditLog.ActionName = actionName;
                    auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? userInfo.Id : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    auditLog.PreviousInformation = null;
                    auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { user });

                    if (_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId) != true)
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return View(userModel);
                    }
                    else
                    {
                        return RedirectToAction("Login");
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(userModel);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View(userModel);
            }
        }

        //[AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginModel, string returnURL)
        {
            string actionName = "Login";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                //EmailVerify
                var userInfo = await _userManager.FindByEmailAsync(loginModel.Email.Trim());

                if (userInfo != null && await _userManager.CheckPasswordAsync(userInfo, loginModel.Password))
                {
                    //var result=await _signInManager.SignInAsync(userInfo, isPersistent: false);
                    var result = await _signInManager.PasswordSignInAsync(loginModel.Email.Trim(), loginModel.Password, loginModel.RememberMe, false);

                    if (result.Succeeded)
                    {
                        if (userInfo.UserStatus != 2)
                        {
                            if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
                            {
                                return Redirect(returnURL);
                            }
                            else
                            {
                                await _signInManager.SignInAsync(userInfo, isPersistent: false);

                                return RedirectToAction("Home");

                                //var token = _userRepository.GenerateJSONWebToken(loginModel);
                                //if (token != null)
                                //{
                                //    return RedirectToAction("Home");
                                //}
                                //else
                                //{
                                //    ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                                //    return View(loginModel);
                                //}

                                //var userRoles = await userManager.GetRolesAsync(userInfo);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, _responseMessage.TerminatedUser.ToString());
                            return View(loginModel);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.WrongLoginInfo.ToString());
                        return View(loginModel);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.WrongLoginInfo.ToString());
                    return View(loginModel);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View(loginModel);
            }
        }

        [Authorize]
        [AcceptVerbs("Get", "Post")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> UserList(string SearchText = "", int pg = 1)
        {
            string actionName = "UserList";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<UserViewModel> userlist = new List<UserViewModel>();
            List<UserViewModel> emptyUserlist = new List<UserViewModel>();
            int recsCount = 0;
            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    userlist = await _userRepository.GetUserListBySearchValue(SearchText, loggedInUserId);
                }
                else
                {
                    userlist = await _userRepository.GetUserList(loggedInUserId);
                }

                if (userlist != null)
                {
                    recsCount = userlist.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var userData = userlist.Skip(0).Take(search_Pager.PageSize).ToList();

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

                        var userData = userlist.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(userData);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(emptyUserlist);
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
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            string actionName = "EditUser";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            RegistrationViewModel registrationViewModel = new RegistrationViewModel();

            try
            {
                var userRoles = _DBContext.Roles.OrderBy(r => r.Name).ToList().Select(rr =>
                            new SelectListItem
                            {
                                Value = rr.Name.ToString(),
                                Text = rr.Name
                            }).ToList();

                var userStatus = _DBContext.UserStatus.OrderBy(r => r.StatusType).ToList().Select(rr =>
                            new SelectListItem { Value = rr.UserStatusId.ToString(), Text = rr.StatusType }).ToList();

                ViewBag.Roles = userRoles;
                ViewBag.Status = userStatus;
                var userData = _userRepository.GetUser(id, loggedInUserId);

                if (userData == null)
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(registrationViewModel);
                }
                else
                {
                    return View(userData);
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
        public async Task<IActionResult> EditUser(RegistrationViewModel userModel)
        {
            string actionName = "EditUser";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                //Existing Role Check By Role Name
                var updatedRoleInfo = await _roleManager.FindByNameAsync(userModel.UserRole);

                if (updatedRoleInfo == null)
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.RoleNotExist.ToString());
                    return View(userModel);
                }
                var previousRoleName = _userRepository.GetUserRoleByUserId(userModel.id, loggedInUserId);

                if (previousRoleName == null)
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(userModel);
                }
                else
                {
                    var previousRoleInfo = await _roleManager.FindByNameAsync(previousRoleName);

                    var existUserStatus = "";

                    //Existing User Check By UserId
                    var userExistsById = await _userManager.FindByIdAsync(userModel.id.ToString());

                    existUserStatus = userExistsById.UserStatus.ToString();

                    if (userExistsById == null)
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.UserNotExist.ToString());
                        return View(userModel);
                    }

                    var currentRoles = await _userManager.GetRolesAsync(userExistsById);
                    await _userManager.RemoveFromRolesAsync(userExistsById, currentRoles);
                    await _roleManager.UpdateAsync(updatedRoleInfo);
                    var result = await _userManager.AddToRoleAsync(userExistsById, updatedRoleInfo.Name);

                    if (result.Succeeded)
                    {
                        AuditLog auditLog = new AuditLog();

                        auditLog.AuditTime = DateTime.UtcNow;
                        auditLog.AreaName = _areaName;
                        auditLog.ControllerName = _controllerName;
                        auditLog.ActionName = actionName;
                        auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        auditLog.PreviousInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { previousRoleInfo }); ;
                        auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { updatedRoleInfo });

                        if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                        {
                            ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                            return View(userModel);
                        }
                        else
                        {
                            if (previousRoleName != updatedRoleInfo.Name)
                            {
                                RoleUpdateHistory roleUpdateHistory = new RoleUpdateHistory();
                                roleUpdateHistory.UserId = userModel.id;
                                roleUpdateHistory.Username = userExistsById.UserName;
                                roleUpdateHistory.Email = userExistsById.Email;
                                roleUpdateHistory.PreviousRole = previousRoleName;
                                roleUpdateHistory.UpdatedRole = updatedRoleInfo.Name;
                                roleUpdateHistory.UpdatedBy = loggedInUserId;
                                roleUpdateHistory.UpdatedDate = DateTime.UtcNow;

                                if (!_roleLogRepository.SaveRoleUpdateLog(roleUpdateHistory, loggedInUserId))
                                {
                                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                                    return View(userModel);
                                }
                                else
                                {
                                    if (userExistsById.UserStatus != userModel.UserStatusId)
                                    {
                                        userExistsById.UserStatus = userModel.UserStatusId;
                                        var updateUserStatus = await _userManager.UpdateAsync(userExistsById);

                                        if (updateUserStatus.Succeeded)
                                        {
                                            UserStatusUpdateLog userStatusUpdateLog = new UserStatusUpdateLog();
                                            userStatusUpdateLog.UserId = userModel.id;
                                            userStatusUpdateLog.Username = userExistsById.UserName;
                                            userStatusUpdateLog.PreviousStatus = Convert.ToInt32(existUserStatus);
                                            userStatusUpdateLog.CurrentStatus = userModel.UserStatusId;
                                            userStatusUpdateLog.UpdatedBy = loggedInUserId;
                                            userStatusUpdateLog.UpdatedDate = DateTime.UtcNow;

                                            if (!_statusLogRepository.SaveUserStatusUpdateLog(userStatusUpdateLog, loggedInUserId))
                                            {
                                                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                                                return View(userModel);
                                            }
                                            else
                                            {
                                                return RedirectToAction("UserList");
                                            }
                                        }
                                    }
                                    return RedirectToAction("UserList");
                                }
                            }
                            else
                            {
                                if (existUserStatus != userModel.UserStatusId.ToString())
                                {
                                    userExistsById.UserStatus = userModel.UserStatusId;
                                    var updateUserStatus = await _userManager.UpdateAsync(userExistsById);

                                    if (updateUserStatus.Succeeded)
                                    {
                                        UserStatusUpdateLog userStatusUpdateLog = new UserStatusUpdateLog();
                                        userStatusUpdateLog.UserId = userModel.id;
                                        userStatusUpdateLog.Username = userExistsById.UserName;
                                        userStatusUpdateLog.PreviousStatus = Convert.ToInt32(existUserStatus);
                                        userStatusUpdateLog.CurrentStatus = userModel.UserStatusId;
                                        userStatusUpdateLog.UpdatedBy = loggedInUserId;
                                        userStatusUpdateLog.UpdatedDate = DateTime.UtcNow;

                                        if (!_statusLogRepository.SaveUserStatusUpdateLog(userStatusUpdateLog, loggedInUserId))
                                        {
                                            ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                                            return View(userModel);
                                        }
                                        else
                                        {
                                            return RedirectToAction("UserList");
                                        }
                                    }
                                }
                                return RedirectToAction("UserList");
                            }
                        }
                    }
                    return View(userModel);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View(userModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        //[AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            string actionName = "IsEmailInUse";
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    return Json(true);
                }
                else
                {
                    return Json(_responseMessage.EmailExist.ToString());
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
