using _3dposManagaement.Database;
using _3dposManagaement.Repository.IRepository.ILogRepository;
using _3dposManagaement.Utility.UserViewModel;
using CommonEntityModel.EntityModel;
using CommonEntityModel.ModelClass;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace _3dposManagaement.Controllers
{

    public class UserController : Controller
    {
        private const string _areaName = "3DPOSManagaement Module";
        private const string _controllerName = "User";
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ResponseMessage _responseMessage = new ResponseMessage();
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly ILogger<UserController> _logger;

        public UserController(RestaurantManagementDBContext DBContext, ILogger<UserController> logger, IErrorLogRepository errorLogRepository, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._logger = logger;
        }

        [Authorize]
        //[Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public IActionResult Home()
        {
            //_logger.LogWarning("THIS IS A CUSTOM MESSAGE");

            //try
            //{
            //    throw new NotImplementedException();
            //}
            //catch (NotImplementedException ex)
            //{
            //    _logger.LogError(ex, ex.Message);
            //}
            return View();
        }

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
                var userInfo = await _userManager.FindByNameAsync(loginModel.Username.Trim());

                if (userInfo != null && await _userManager.CheckPasswordAsync(userInfo, loginModel.Password))
                {
                    var result = await _signInManager.PasswordSignInAsync(loginModel.Username.Trim(), loginModel.Password, loginModel.RememberMe, false);

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

                                string RestaurantCode = "";
                                var claim = new Claim(RestaurantCode, userInfo.RestaurantCode);
                                var result2 = await _userManager.AddClaimAsync(userInfo, claim);

                                return RedirectToAction("ProductList", "Inventory");
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

    }
}

