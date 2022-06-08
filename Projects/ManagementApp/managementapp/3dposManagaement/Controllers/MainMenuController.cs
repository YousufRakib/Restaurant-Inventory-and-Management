using _3dposManagaement.Database;
using _3dposManagaement.Repository.IRepository.IInventoryRepository;
using _3dposManagaement.Repository.IRepository.IItemCategoryRepository;
using _3dposManagaement.Repository.IRepository.IItemRepository;
using _3dposManagaement.Repository.IRepository.ILogRepository;
using _3dposManagaement.Repository.IRepository.IMenuLogRepository;
using _3dposManagaement.Repository.IRepository.IMenuRepository;
using _3dposManagaement.Repository.IRepository.IProductCategoryRepository;
using _3dposManagaement.Repository.IRepository.IProductRepository;
using _3dposManagaement.Repository.IRepository.IUnitTypeRepository;
using _3dposManagaement.Repository.IRepository.IVariantRepository;
using _3dposManagaement.Repository.IRepository.IVarificationCodeRepository;
using _3dposManagaement.Utility.CommonModel;
using _3dposManagaement.Utility.MenuModel;
using ClosedXML.Excel;
using CommonEntityModel.EntityModel;
using CommonEntityModel.ModelClass;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
//using System.Web.Mvc;

namespace _3dposManagaement.Controllers
{
    [Authorize]
    public class MainMenuController : Controller
    {
        private const string _areaName = "3DPOSRManagement Module";
        private const string _controllerName = "MainMenu";
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ResponseMessage _responseMessage = new ResponseMessage();
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitTypeRepository _unitTypeRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly IItemCategoryRepository _itemCategoryRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IVariantRepository _variantRepository;
        private readonly IMenuLogRepository _menuLogRepository;
        private readonly IVarificationCodeRepository _varificationCodeRepository;


        public MainMenuController(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository, IAuditLogRepository auditLogRepository, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IProductCategoryRepository productCategoryRepository, IProductRepository productRepository, IUnitTypeRepository unitTypeRepository, IInventoryRepository inventoryRepository, IMenuRepository menuRepository, IItemCategoryRepository itemCategoryRepository, IItemRepository itemRepository, IVariantRepository variantRepository, IMenuLogRepository menuLogRepository, IVarificationCodeRepository varificationCodeRepository)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._auditLogRepository = auditLogRepository;
            this._configuration = configuration;
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
            this._productCategoryRepository = productCategoryRepository;
            this._productRepository = productRepository;
            this._unitTypeRepository = unitTypeRepository;
            this._inventoryRepository = inventoryRepository;
            this._menuRepository = menuRepository;
            this._itemCategoryRepository = itemCategoryRepository;
            this._itemRepository = itemRepository;
            this._variantRepository = variantRepository;
            this._menuLogRepository = menuLogRepository;
            this._varificationCodeRepository = varificationCodeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> MainMenuList(string SearchText = "", int pg = 1)
        {
            string actionName = "MainMenuList";
            var path = "MenuManagement > MenuList";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            MenuMargeViewModel menuViewModel = new MenuMargeViewModel();
            List<MenuViewModel> menuList = new List<MenuViewModel>();
            int recsCount = 0;
            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    menuList = await _menuRepository.GetMenuListBySearchValue(SearchText, loggedInUserId, restaurantCode.ToString());
                }
                else
                {
                    menuList = await _menuRepository.GetMenuList(loggedInUserId, restaurantCode.ToString());
                }


                menuViewModel.menuList = menuList;

                if (menuViewModel != null)
                {
                    recsCount = menuViewModel.menuList.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var menuData = menuViewModel.menuList.Skip(0).Take(search_Pager.PageSize).ToList();

                        MenuMargeViewModel menuViewModelData = new MenuMargeViewModel();
                        menuViewModelData.menuList = menuData;

                        ViewBag.Message = "No menu added yet!";
                        ViewBag.Path = path;
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(menuViewModelData);
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

                        var menuData = menuViewModel.menuList.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        MenuMargeViewModel menuViewModelData = new MenuMargeViewModel();
                        menuViewModelData.menuList = menuData;

                        ViewBag.Path = path;
                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(menuViewModelData);
                    }
                }
                else
                {
                    MenuMargeViewModel menuViewModelData = new MenuMargeViewModel();
                    menuViewModelData.menuList = new List<MenuViewModel>();
                    //ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    ViewBag.Path = path;
                    return View(menuViewModelData);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                ViewBag.Path = path;
                return View();
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetMenuById(int menuId)
        {
            string value = string.Empty;
            string actionName = "GetMenuById";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            MenuMargeViewModel menu = new MenuMargeViewModel();
            try
            {
                menu = await _menuRepository.GetMenuById(loggedInUserId, restaurantCode.ToString(), menuId);
                value = JsonConvert.SerializeObject(menu, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                return Json(value);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                throw ex;
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetMenuImage(int menuId)
        {
            string actionName = "GetImage";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                var fileSavePath = "";
                MenuMargeViewModel menu = new MenuMargeViewModel();

                menu = await _menuRepository.GetMenuById(loggedInUserId, restaurantCode.ToString(), menuId);

                fileSavePath = menu.MenuImagePath;


                if (menu.MenuImagePath != "" || menu.MenuImagePath == null)
                {
                    if (!System.IO.File.Exists(fileSavePath))
                    {
                        fileSavePath = "~/wwwroot/Files/avatarEmp.png";
                    }

                    var image = Image.FromFile(fileSavePath);

                    if (ImageFormat.Jpeg.Equals(image.RawFormat))
                    {
                        // JPEG
                        using (var memoryStream = new MemoryStream())
                        {
                            image.Save(memoryStream, ImageFormat.Jpeg);
                            image.Dispose();
                            return base.File(memoryStream.ToArray(), "image/jpeg");
                        }
                    }
                    else if (ImageFormat.Png.Equals(image.RawFormat))
                    {
                        // PNG
                        using (var memoryStream = new MemoryStream())
                        {
                            image.Save(memoryStream, ImageFormat.Png);
                            image.Dispose();
                            return base.File(memoryStream.ToArray(), "image/png");
                        }
                    }
                    else if (ImageFormat.Gif.Equals(image.RawFormat))
                    {
                        // GIF
                        using (var memoryStream = new MemoryStream())
                        {
                            image.Save(memoryStream, ImageFormat.Gif);
                            image.Dispose();
                            return base.File(memoryStream.ToArray(), "image/gif");
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return null;
            }
        }
        [HttpPost]
        public async Task<JsonResult> DeleteMenuRecord(int menuId,string code)
        {
            string actionName = "DeleteMenuRecord";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var isDelete = await _menuRepository.DeleteMenuById(loggedInUserId, restaurantCode.ToString(), menuId,code);
                if (isDelete == "True")
                {
                    return Json(new Confirmation { output = "success" });
                }
                if (isDelete == "False")
                {
                    return Json(new Confirmation { output = "insuccess" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }
        [HttpPost]
        public async Task<JsonResult> ChangeMenuStatus(int menuId)
        {
            string actionName = "ChangeMenuStatus";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var isStatusChanged = await _menuRepository.ChangeMenuStatusById(loggedInUserId, restaurantCode.ToString(), menuId);
                if (isStatusChanged == "True")
                {
                    return Json(new Confirmation { output = "success" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }
        [HttpPost]
        public async Task<JsonResult> SaveMenu()
        {
            var imageFile = Request.Form.Files["MenuImageFile"];
            var data = HttpContext.Request.Form["dataModel"];

            MenuViewModel model = JsonConvert.DeserializeObject<MenuViewModel>(data);
            
            string actionName = "SaveMenu";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                Menu menu = await _menuRepository.SaveMenu(model, imageFile, loggedInUserId, restaurantCode.ToString());
                if (menu != null)
                {
                    return Json(new Confirmation { output = "success" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }
        [HttpPost]
        public async Task<JsonResult> EditMenu()
        {
            var imageFile = Request.Form.Files["MenuImageFile"];
            var data = HttpContext.Request.Form["dataModel"];

            MenuViewModel model = JsonConvert.DeserializeObject<MenuViewModel>(data);

            string actionName = "EditMenu";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                Menu menu = await _menuRepository.UpdateMenu(model, imageFile, loggedInUserId, restaurantCode.ToString());
                if (menu != null)
                {
                    return Json(new Confirmation { output = "success" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> ItemCategoryList(string SearchText = "", int pg = 1)
        {
            string actionName = "ItemCategoryList";
            var path = "MenuManagement > ItemCategoryList";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            ItemCategoryMargeViewModel itemCategoryMargeViewModel = new ItemCategoryMargeViewModel();
            List<ItemCategoryViewModel> itemCategoryMenuList = new List<ItemCategoryViewModel>();
            int recsCount = 0;
            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    itemCategoryMenuList = await _itemCategoryRepository.ItemCategoryListBySearchValue(SearchText, loggedInUserId, restaurantCode.ToString());
                }
                else
                {
                    itemCategoryMenuList = await _itemCategoryRepository.ItemCategoryList(loggedInUserId, restaurantCode.ToString());
                }


                itemCategoryMargeViewModel.ItemCategoryList = itemCategoryMenuList;

                if (itemCategoryMargeViewModel != null)
                {
                    recsCount = itemCategoryMargeViewModel.ItemCategoryList.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var itemCategoryData = itemCategoryMargeViewModel.ItemCategoryList.Skip(0).Take(search_Pager.PageSize).ToList();

                        ItemCategoryMargeViewModel itemCategoryMargeData = new ItemCategoryMargeViewModel();
                        itemCategoryMargeData.ItemCategoryList = itemCategoryData;
                        itemCategoryMargeData.ItemCategoryViewModel = new ItemCategoryViewModel();
                        itemCategoryMargeData.ItemCategoryWithMenuList = new List<ItemCategoryWithMenuViewModel>();

                        ViewBag.Message = "No item category added yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;
                        ViewBag.Path = path;

                        return View(itemCategoryMargeData);
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
                        var itemCategoryData = itemCategoryMargeViewModel.ItemCategoryList.Skip(recSkip).Take(search_Pager.PageSize).ToList();


                        ItemCategoryMargeViewModel itemCategoryMargeData = new ItemCategoryMargeViewModel();
                        itemCategoryMargeData.ItemCategoryList = itemCategoryData;
                        itemCategoryMargeData.ItemCategoryViewModel = new ItemCategoryViewModel();
                        itemCategoryMargeData.ItemCategoryWithMenuList = new List<ItemCategoryWithMenuViewModel>();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;
                        ViewBag.Path = path;
                        return View(itemCategoryMargeData);
                    }
                }
                else
                {
                    ItemCategoryMargeViewModel itemCategoryMargeData = new ItemCategoryMargeViewModel();
                    itemCategoryMargeData.ItemCategoryList = new List<ItemCategoryViewModel>();
                    itemCategoryMargeData.ItemCategoryViewModel = new ItemCategoryViewModel();
                    itemCategoryMargeData.ItemCategoryWithMenuList = new List<ItemCategoryWithMenuViewModel>();
                    ViewBag.Path = path;
                    return View(itemCategoryMargeData);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                ViewBag.Path = path;
                return View();
            }
        }
        [HttpPost]
        public async Task<JsonResult> AddItemCategory()
        {
            var imageFile = Request.Form.Files["itemCategoryImageFile"];
            var itemCategoryData = HttpContext.Request.Form["dataModel"];
            var relationalData= HttpContext.Request.Form["menuListModel"];

            ItemCategoryViewModel itemCategoryDataModel = JsonConvert.DeserializeObject<ItemCategoryViewModel>(itemCategoryData);
            List<ItemCategoryWithMenuViewModel> relationalDataModel = JsonConvert.DeserializeObject<List<ItemCategoryWithMenuViewModel>>(relationalData);

            ItemCategoryMargeViewModel itemCategoryMargeViewModel = new ItemCategoryMargeViewModel();
            itemCategoryMargeViewModel.ItemCategoryWithMenuList = relationalDataModel;
            itemCategoryMargeViewModel.ItemCategoryViewModel = itemCategoryDataModel;

            string actionName = "AddItemCategory";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                ItemCategoryMargeViewModel returnResult = await _itemCategoryRepository.SaveItemCategory(itemCategoryMargeViewModel, imageFile, loggedInUserId, restaurantCode.ToString());
                if (returnResult != null)
                {
                    return Json(new Confirmation { output = "success" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetItemCategoryById(int categoryId)
        {
            string value = string.Empty;
            string actionName = "GetItemCategoryById";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            ItemCategoryMargeViewModel itemCategoryMargeViewModel = new ItemCategoryMargeViewModel();
            try
            {
                itemCategoryMargeViewModel = await _itemCategoryRepository.GetItemCategoryById(loggedInUserId, restaurantCode.ToString(), categoryId);
                value = JsonConvert.SerializeObject(itemCategoryMargeViewModel, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                return Json(value);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                throw ex;
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetItemCategoryImage(int categoryId)
        {
            string actionName = "GetItemCategoryImage";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                var fileSavePath = "";
                ItemCategoryMargeViewModel itemCategoryMargeViewModel = new ItemCategoryMargeViewModel();

                itemCategoryMargeViewModel = await _itemCategoryRepository.GetItemCategoryById(loggedInUserId, restaurantCode.ToString(), categoryId);

                fileSavePath = itemCategoryMargeViewModel.CategoryIconPath;


                if (itemCategoryMargeViewModel.CategoryIconPath != "" || itemCategoryMargeViewModel.CategoryIconPath == null)
                {
                    if (!System.IO.File.Exists(fileSavePath))
                    {
                        fileSavePath = "~/wwwroot/Files/avatarEmp.png";
                    }

                    var image = Image.FromFile(fileSavePath);

                    if (ImageFormat.Jpeg.Equals(image.RawFormat))
                    {
                        // JPEG
                        using (var memoryStream = new MemoryStream())
                        {
                            image.Save(memoryStream, ImageFormat.Jpeg);
                            image.Dispose();
                            return base.File(memoryStream.ToArray(), "image/jpeg");
                        }
                    }
                    else if (ImageFormat.Png.Equals(image.RawFormat))
                    {
                        // PNG
                        using (var memoryStream = new MemoryStream())
                        {
                            image.Save(memoryStream, ImageFormat.Png);
                            image.Dispose();
                            return base.File(memoryStream.ToArray(), "image/png");
                        }
                    }
                    else if (ImageFormat.Gif.Equals(image.RawFormat))
                    {
                        // GIF
                        using (var memoryStream = new MemoryStream())
                        {
                            image.Save(memoryStream, ImageFormat.Gif);
                            image.Dispose();
                            return base.File(memoryStream.ToArray(), "image/gif");
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return null;
            }
        }
        [HttpPost]
        public async Task<JsonResult> EditItemCategory()
        {
            var imageFile = Request.Form.Files["ItemCategoryImageFile"];
            var itemCategoryData = HttpContext.Request.Form["dataModel"];
            var relationalData = HttpContext.Request.Form["menuListModel"];

            ItemCategoryViewModel itemCategoryDataModel = JsonConvert.DeserializeObject<ItemCategoryViewModel>(itemCategoryData);
            List<ItemCategoryWithMenuViewModel> relationalDataModel = JsonConvert.DeserializeObject<List<ItemCategoryWithMenuViewModel>>(relationalData);

            ItemCategoryMargeViewModel itemCategoryMargeViewModel = new ItemCategoryMargeViewModel();
            itemCategoryMargeViewModel.ItemCategoryWithMenuList = relationalDataModel;
            itemCategoryMargeViewModel.ItemCategoryViewModel = itemCategoryDataModel;

            string actionName = "EditItemCategory";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                ItemCategoryMargeViewModel returnData = await _itemCategoryRepository.UpdateItemCategory(itemCategoryMargeViewModel, imageFile, loggedInUserId, restaurantCode.ToString());
                if (returnData != null)
                {
                    return Json(new Confirmation { output = "success" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }
        [HttpPost]
        public async Task<JsonResult> DeleteItemCategory(int categoryId, string code)
        {
            string actionName = "DeleteItemCategory";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var isDelete = await _itemCategoryRepository.DeleteItemCategoryById(loggedInUserId, restaurantCode.ToString(), categoryId,code);
                if (isDelete == "True")
                {
                    return Json(new Confirmation { output = "success" });
                }
                if (isDelete == "False")
                {
                    return Json(new Confirmation { output = "insuccess" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }
        [HttpPost]
        public async Task<JsonResult> ChangeCategoryStatus(int categoryId)
        {
            string actionName = "ChangeCategoryStatus";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var isStatusChanged = await _itemCategoryRepository.ChangeCategoryStatusById(loggedInUserId, restaurantCode.ToString(), categoryId);
                if (isStatusChanged == "True")
                {
                    return Json(new Confirmation { output = "success" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetMenuListForCategory(int categoryId)
        {
            string value = string.Empty;
            string actionName = "GetMenuList";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<ItemCategoryWithMenuViewModel> itemCategoryWithMenuViewModel = new List<ItemCategoryWithMenuViewModel>();
            try
            {
                itemCategoryWithMenuViewModel = await _itemCategoryRepository.GetMenuList(loggedInUserId, restaurantCode.ToString(), categoryId);

                return Json(itemCategoryWithMenuViewModel);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                throw ex;
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetMenuListForItem(int itemId)
        {
            string value = string.Empty;
            string actionName = "GetMenuListForItem";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<ItemCategoryWithMenuViewModel> itemCategoryWithMenuViewModel = new List<ItemCategoryWithMenuViewModel>();
            try
            {
                itemCategoryWithMenuViewModel = await _itemRepository.GetMenuList(loggedInUserId, restaurantCode.ToString(), itemId);

                return Json(itemCategoryWithMenuViewModel);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                throw ex;
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetItemCategoryListForItem(int itemId)
        {
            string value = string.Empty;
            string actionName = "GetItemCategoryListForItem";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<ItemWithCategoryModel> itemWithCategoryModel = new List<ItemWithCategoryModel>();
            try
            {
                itemWithCategoryModel = await _itemRepository.GetCategoryList(loggedInUserId, restaurantCode.ToString(), itemId);

                return Json(itemWithCategoryModel);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                throw ex;
            }
        }


        [HttpGet]
        public async Task<IActionResult> ItemList(string SearchText = "", int pg = 1)
        {
            string actionName = "ItemList";
            var path = "MenuManagement > ItemList";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            ItemMargeViewModel itemMargeViewModel = new ItemMargeViewModel();
            List<ItemCategoryViewModel> itemCategoryMenuList = new List<ItemCategoryViewModel>();
            int recsCount = 0;
            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    itemMargeViewModel = await _itemRepository.GetItemListBySearchValue(SearchText, loggedInUserId, restaurantCode.ToString());
                }
                else
                {
                    itemMargeViewModel = await _itemRepository.GetItemList(loggedInUserId, restaurantCode.ToString());
                }

                if (itemMargeViewModel != null)
                {
                    recsCount = itemMargeViewModel.ItemListModels.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        itemMargeViewModel.ItemListModels = itemMargeViewModel.ItemListModels.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No item added yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;

                        List<VariantMaster> variantMasterList = await _itemRepository.VariantMasterList(loggedInUserId, restaurantCode);
                        ViewBag.VariantMasterList = new SelectList(variantMasterList, "VariantMasterId", "VariantMasterName");
                        ViewBag.Path = path;
                        return View(itemMargeViewModel);
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
                        itemMargeViewModel.ItemListModels = itemMargeViewModel.ItemListModels.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;

                        List<VariantMaster> variantMasterList = await _itemRepository.VariantMasterList(loggedInUserId, restaurantCode);
                        ViewBag.VariantMasterList = new SelectList(variantMasterList, "VariantMasterId", "VariantMasterName");
                        ViewBag.Path = path;
                        return View(itemMargeViewModel);
                    }
                }
                else
                {
                    ItemMargeViewModel emptyItemMargeViewModel = new ItemMargeViewModel();
                    emptyItemMargeViewModel.ItemListModels = new List<ItemListModel>();
                    emptyItemMargeViewModel.ItemListModel = new ItemListModel();
                    emptyItemMargeViewModel.VariantAndPriceModel = new List<VariantAndPriceModel>();
                    emptyItemMargeViewModel.ItemWithMenuModel = new List<ItemWithMenuModel>();
                    emptyItemMargeViewModel.ItemWithCategoryModel = new List<ItemWithCategoryModel>();
                    emptyItemMargeViewModel.VariantMaster = new List<VariantMaster>();
                    emptyItemMargeViewModel.VariantPriceModel = new List<VariantPriceModel>();

                    List<VariantMaster> variantMasterList = await _itemRepository.VariantMasterList(loggedInUserId, restaurantCode);
                    ViewBag.VariantMasterList = new SelectList(variantMasterList, "VariantMasterId", "VariantMasterName");
                    ViewBag.Path = path;
                    return View(emptyItemMargeViewModel);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                ViewBag.Path = path;
                return View();
            }
        }

        [HttpPost]
        public async Task<JsonResult> AddItem()
        {
            var imageFile = Request.Form.Files["itemImageFile"]; 
            var itemData = HttpContext.Request.Form["dataModel"];
            var relationalMenuData = HttpContext.Request.Form["menuListModel"];
            var relationalCategoryData = HttpContext.Request.Form["categoryListModel"];
            var variantPriceData = HttpContext.Request.Form["variantListModel"];

            ItemListModel itemListModel = JsonConvert.DeserializeObject<ItemListModel>(itemData); 
            List<ItemWithMenuModel> relationalMenuDataModel = JsonConvert.DeserializeObject<List<ItemWithMenuModel>>(relationalMenuData);
            List<ItemWithCategoryModel> relationalategoryDataModel = JsonConvert.DeserializeObject<List<ItemWithCategoryModel>>(relationalCategoryData);
            List<VariantPriceModel> variantPriceDataModel = JsonConvert.DeserializeObject<List<VariantPriceModel>>(variantPriceData);

            ItemMargeViewModel ItemMargeViewModel = new ItemMargeViewModel();
            ItemMargeViewModel.ItemWithMenuModel = relationalMenuDataModel;
            ItemMargeViewModel.ItemWithCategoryModel = relationalategoryDataModel;
            ItemMargeViewModel.ItemListModel = itemListModel; 
            ItemMargeViewModel.VariantPriceModel = variantPriceDataModel;

            string actionName = "AddItem";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                ItemMargeViewModel returnResult = await _itemRepository.SaveItem(ItemMargeViewModel, imageFile, loggedInUserId, restaurantCode.ToString());
                if (returnResult != null)
                {
                    return Json(new Confirmation { output = "success" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });

                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetItemById(int itemId)
        {
            string value = string.Empty;
            string actionName = "GetItemById";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            ItemMargeViewModel itemMargeViewModel = new ItemMargeViewModel();
            try
            {
                itemMargeViewModel = await _itemRepository.GetItemById(loggedInUserId, restaurantCode.ToString(), itemId);
                value = JsonConvert.SerializeObject(itemMargeViewModel, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                return Json(value);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                throw ex;
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetVariantList(int variantMasterId)
        {
            string value = string.Empty;
            string actionName = "GetVariantList";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                List<ItemVariant> variantMasterList = await _itemRepository.VariantList(loggedInUserId, restaurantCode, variantMasterId);
                
                return Json(variantMasterList);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                throw ex;
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetVariantListWithPrice(int variantMasterId,int itemId)
        {
            string value = string.Empty;
            string actionName = "GetVariantList";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                List<ItemVariant> variantMasterList = await _itemRepository.VariantListWithPrice(loggedInUserId, restaurantCode, variantMasterId, itemId);

                return Json(variantMasterList);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                throw ex;
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetItemImage(int itemId)
        {
            string actionName = "GetItemImage";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                var fileSavePath = "";

                ItemMargeViewModel itemMargeViewModel =await _itemRepository.GetItemById(loggedInUserId, restaurantCode.ToString(), itemId);

                fileSavePath = itemMargeViewModel.ItemImagePath;


                if (itemMargeViewModel.ItemImagePath != "" || itemMargeViewModel.ItemImagePath == null)
                {
                    if (!System.IO.File.Exists(fileSavePath))
                    {
                        fileSavePath = "~/wwwroot/Files/avatarEmp.png";
                    }

                    var image = Image.FromFile(fileSavePath);

                    if (ImageFormat.Jpeg.Equals(image.RawFormat))
                    {
                        // JPEG
                        using (var memoryStream = new MemoryStream())
                        {
                            image.Save(memoryStream, ImageFormat.Jpeg);
                            image.Dispose();
                            return base.File(memoryStream.ToArray(), "image/jpeg");
                        }
                    }
                    else if (ImageFormat.Png.Equals(image.RawFormat))
                    {
                        // PNG
                        using (var memoryStream = new MemoryStream())
                        {
                            image.Save(memoryStream, ImageFormat.Png);
                            image.Dispose();
                            return base.File(memoryStream.ToArray(), "image/png");
                        }
                    }
                    else if (ImageFormat.Gif.Equals(image.RawFormat))
                    {
                        // GIF
                        using (var memoryStream = new MemoryStream())
                        {
                            image.Save(memoryStream, ImageFormat.Gif);
                            image.Dispose();
                            return base.File(memoryStream.ToArray(), "image/gif");
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return null;
            }
        }

        [HttpPost]
        public async Task<JsonResult> EditItem()
        {
            var imageFile = Request.Form.Files["itemImageFile"];
            var itemData = HttpContext.Request.Form["dataModel"];
            var relationalMenuData = HttpContext.Request.Form["menuListModel"];
            var relationalCategoryData = HttpContext.Request.Form["categoryListModel"];
            var variantPriceData = HttpContext.Request.Form["variantListModel"];

            ItemListModel itemListModel = JsonConvert.DeserializeObject<ItemListModel>(itemData);
            List<ItemWithMenuModel> relationalMenuDataModel = JsonConvert.DeserializeObject<List<ItemWithMenuModel>>(relationalMenuData);
            List<ItemWithCategoryModel> relationalategoryDataModel = JsonConvert.DeserializeObject<List<ItemWithCategoryModel>>(relationalMenuData);
            List<VariantPriceModel> variantPriceDataModel = JsonConvert.DeserializeObject<List<VariantPriceModel>>(variantPriceData);

            ItemMargeViewModel ItemMargeViewModel = new ItemMargeViewModel();
            ItemMargeViewModel.ItemWithMenuModel = relationalMenuDataModel;
            ItemMargeViewModel.ItemWithCategoryModel = relationalategoryDataModel;
            ItemMargeViewModel.ItemListModel = itemListModel;
            ItemMargeViewModel.VariantPriceModel = variantPriceDataModel;

            string actionName = "EditItem";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                ItemMargeViewModel returnData = await _itemRepository.UpdateItem(ItemMargeViewModel, imageFile, loggedInUserId, restaurantCode.ToString());
                if (returnData != null)
                {
                    return Json(new Confirmation { output = "success" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteItem(int itemId, string code)
        {
            string actionName = "DeleteItem";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var isDelete = await _itemRepository.DeleteItemById(loggedInUserId, restaurantCode.ToString(), itemId,code);
                if (isDelete == "True")
                {
                    return Json(new Confirmation { output = "success" });
                }
                if (isDelete == "False")
                {
                    return Json(new Confirmation { output = "insuccess" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
                
                //return Json(result);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ChangeItemStatus(int itemId)
        {
            string actionName = "ChangeItemStatus";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var isStatusChanged = await _itemRepository.ChangeItemStatusById(loggedInUserId, restaurantCode.ToString(), itemId);
                if (isStatusChanged == "True")
                {
                    return Json(new Confirmation { output = "success" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }



        [HttpGet]
        public async Task<IActionResult> VariantList(string SearchText = "", int pg = 1)
        {
            string actionName = "ItemList";
            var path = "MenuManagement > VariantList";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            VariantMargeViewModel variantMargeViewModel = new VariantMargeViewModel();
            List<ItemCategoryViewModel> itemCategoryMenuList = new List<ItemCategoryViewModel>();
            int recsCount = 0;
            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    variantMargeViewModel = await _variantRepository.GetVariantListBySearchValue(SearchText, loggedInUserId, restaurantCode.ToString());
                }
                else
                {
                    variantMargeViewModel = await _variantRepository.GetVariantList(loggedInUserId, restaurantCode.ToString());
                }

                if (variantMargeViewModel != null)
                {
                    recsCount = variantMargeViewModel.VariantViewModelList.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        variantMargeViewModel.VariantViewModelList = variantMargeViewModel.VariantViewModelList.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No variant added yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;
                        ViewBag.Path = path;
                        return View(variantMargeViewModel);
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
                        variantMargeViewModel.VariantViewModelList = variantMargeViewModel.VariantViewModelList.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;
                        ViewBag.Path = path;
                        return View(variantMargeViewModel);
                    }
                }
                else
                {
                    VariantMargeViewModel emptyVariantMargeViewModel = new VariantMargeViewModel();
                    emptyVariantMargeViewModel.VariantViewModelList = new List<VariantViewModel>();
                    emptyVariantMargeViewModel.VariantViewModel = new VariantViewModel();
                    ViewBag.Path = path;
                    return View(emptyVariantMargeViewModel);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                ViewBag.Path = path;
                return View();
            }
        }

        [HttpPost]
        public async Task<JsonResult> AddVariant()
        {
            var variantMasterData = HttpContext.Request.Form["dataModel"];
            var variantListData = HttpContext.Request.Form["variantListModel"];

            VariantViewModel variantMasterDataModel = JsonConvert.DeserializeObject<VariantViewModel>(variantMasterData);
            List<VariantViewModel> variantListDataModel = JsonConvert.DeserializeObject<List<VariantViewModel>>(variantListData);

            VariantMargeViewModel VariantMargeViewModel = new VariantMargeViewModel();
            VariantMargeViewModel.VariantViewModelList = variantListDataModel;
            VariantMargeViewModel.VariantViewModel = variantMasterDataModel;

            bool result = false;
            string actionName = "AddVariant";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                VariantMargeViewModel returnResult = await _variantRepository.SaveVariant(VariantMargeViewModel, loggedInUserId, restaurantCode.ToString());
                
                if (returnResult != null)
                {
                    return Json(new Confirmation { output = "success" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ChangeVariantStatus(int variantId)
        {
            string actionName = "ChangeVariantStatus";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var isStatusChanged = await _variantRepository.ChangeVariantStatusById(loggedInUserId, restaurantCode.ToString(), variantId);
                if (isStatusChanged == "True")
                {
                    return Json(new Confirmation { output = "success" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }



        [HttpGet]
        public async Task<IActionResult> UserVarificationCodeList(string SearchText = "", int pg = 1)
        {
            string actionName = "UserVarificationCodeList";
            var path = "MenuManagement > UserVarificationCodeList";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            VarificationCodeMargeViewModel varificationCodeMargeViewModel = new VarificationCodeMargeViewModel();
            List<VarificationCodeViewModel> varificationCodeList = new List<VarificationCodeViewModel>();
            int recsCount = 0;
            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    varificationCodeList = await _varificationCodeRepository.GetVarificationCodeListBySearchValue(SearchText, loggedInUserId, restaurantCode.ToString());
                }
                else
                {
                    varificationCodeList = await _varificationCodeRepository.GetVarificationCodeList(loggedInUserId, restaurantCode.ToString());
                }


                varificationCodeMargeViewModel.varificationCodeViewModel = varificationCodeList;

                if (varificationCodeMargeViewModel != null)
                {
                    recsCount = varificationCodeMargeViewModel.varificationCodeViewModel.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var varificationCodeData = varificationCodeMargeViewModel.varificationCodeViewModel.Skip(0).Take(search_Pager.PageSize).ToList();

                        VarificationCodeMargeViewModel varificationCodeMargeModel = new VarificationCodeMargeViewModel();
                        varificationCodeMargeModel.varificationCodeViewModel = varificationCodeData;

                        ViewBag.Message = "No varification code added for user!";
                        ViewBag.Path = path;
                        this.ViewBag.searchBar_Pager = search_Pager;

                        List<VarificationCodeUserModel> userList = await _varificationCodeRepository.NotExistUserInVarificationCode(loggedInUserId, restaurantCode);
                        ViewBag.UserList = new SelectList(userList, "Username", "Username");

                        return View(varificationCodeMargeModel);
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

                        var varificationCodeData = varificationCodeMargeViewModel.varificationCodeViewModel.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        VarificationCodeMargeViewModel varificationCodeMargeModel = new VarificationCodeMargeViewModel();
                        varificationCodeMargeModel.varificationCodeViewModel = varificationCodeData;

                        ViewBag.Path = path;
                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;

                        List<VarificationCodeUserModel> userList = await _varificationCodeRepository.NotExistUserInVarificationCode(loggedInUserId, restaurantCode);
                        ViewBag.UserList = new SelectList(userList, "Username", "Username");

                        return View(varificationCodeMargeModel);
                    }
                }
                else
                {
                    VarificationCodeMargeViewModel varificationCodeMargeModel = new VarificationCodeMargeViewModel();
                    varificationCodeMargeModel.varificationCodeViewModel = new List<VarificationCodeViewModel>();

                    List<VarificationCodeUserModel> userList = await _varificationCodeRepository.NotExistUserInVarificationCode(loggedInUserId, restaurantCode);
                    ViewBag.UserList = new SelectList(userList, "Username", "Username");

                    ViewBag.Path = path;
                    return View(varificationCodeMargeModel);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());

                List<VarificationCodeUserModel> userList = await _varificationCodeRepository.NotExistUserInVarificationCode(loggedInUserId, restaurantCode);
                ViewBag.UserList = new SelectList(userList, "Username", "Username");

                ViewBag.Path = path;
                return View();
            }
        }
       
        [HttpGet]
        public async Task<JsonResult> GetUserVarificationCodeById(int codeId)
        {
            string value = string.Empty;
            string actionName = "GetUserVarificationCodeById";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            VarificationCodeViewModel varificationCodeModel = new VarificationCodeViewModel();
            try
            {
                varificationCodeModel = await _varificationCodeRepository.GetVarificationCodeById(loggedInUserId, restaurantCode.ToString(), codeId);
                value = JsonConvert.SerializeObject(varificationCodeModel, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                return Json(value);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                throw ex;
            }
        }

        [HttpPost]
        public async Task<JsonResult> SaveUserVarificationCode()
        {
            var data = HttpContext.Request.Form["dataModel"];

            VarificationCodeViewModel model = JsonConvert.DeserializeObject<VarificationCodeViewModel>(data);

            string actionName = "SaveUserVarificationCode";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                VarificationCodeViewModel saveVarificationCodeData = await _varificationCodeRepository.SaveVarificationCode(model, loggedInUserId, restaurantCode.ToString());
                if (saveVarificationCodeData != null)
                {
                    return Json(new Confirmation { output = "success" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }
        
        [HttpPost]
        public async Task<JsonResult> EditUserVarificationCode()
        {
            var data = HttpContext.Request.Form["dataModel"];

            VarificationCodeViewModel model = JsonConvert.DeserializeObject<VarificationCodeViewModel>(data);

            string actionName = "EditUserVarificationCode";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                VarificationCodeViewModel saveVarificationCodeData = await _varificationCodeRepository.UpdateVarificationCode(model, loggedInUserId, restaurantCode.ToString());
                if (saveVarificationCodeData != null)
                {
                    return Json(new Confirmation { output = "success" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteVarificationCode(int codeId,string code)
        {
            string actionName = "DeleteVarificationCode";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var isDelete = await _varificationCodeRepository.DeleteVarificationCodeById(loggedInUserId, restaurantCode.ToString(), codeId, code);
                if (isDelete == "True")
                {
                    return Json(new Confirmation { output = "success" });
                }
                if (isDelete == "False")
                {
                    return Json(new Confirmation { output = "insuccess" });
                }
                else
                {
                    return Json(new Confirmation { output = "error" });
                }

                //return Json(result);
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return Json(new Confirmation { output = "error" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> MenuLogList(string SearchText = "", int pg = 1)
        {
            string actionName = "MenuLogList";
            var path = "MenuManagement > MenuLogList";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<MenuLogModel> menuLogs = new List<MenuLogModel>();
            int recsCount = 0;
            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    menuLogs = await _menuLogRepository.GetMenuLogListBySearchValue(SearchText, loggedInUserId, restaurantCode.ToString());
                }
                else
                {
                    menuLogs = await _menuLogRepository.GetMenuLogList(loggedInUserId, restaurantCode.ToString());
                }

                if (menuLogs != null)
                {
                    recsCount = menuLogs.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        menuLogs = menuLogs.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No log added yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;
                        ViewBag.Path = path;
                        return View(menuLogs);
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
                        menuLogs = menuLogs.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;
                        ViewBag.Path = path;
                        return View(menuLogs);
                    }
                }
                else
                {
                    List<MenuLogModel> emptyMenuLog = new List<MenuLogModel>();
                    ViewBag.Path = path;
                    return View(emptyMenuLog);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                ViewBag.Path = path;
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExporDataToCSV(string txtSelectedDateFrom = "", string txtSelectedDateTo = "")
        {
            string actionName = "ExporDataToCSV";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<MenuLogModel> menuLogs = new List<MenuLogModel>();

            if((txtSelectedDateFrom == "" || txtSelectedDateFrom == null) && (txtSelectedDateTo == "" || txtSelectedDateTo == null))
            {
                menuLogs = await _menuLogRepository.GetMenuLogList(loggedInUserId, restaurantCode.ToString()); 
            }
            else
            {
                menuLogs = await _menuLogRepository.GetDateWiseMenuLogList(loggedInUserId, restaurantCode.ToString(), txtSelectedDateFrom, txtSelectedDateTo);
            }

            

            var builder = new StringBuilder();
            builder.AppendLine("Sl,ChangeType,ChangeItemName,OldValue,UpdatedValue,CreatedByName,CreatedDate");

            foreach (var data in menuLogs)
            {
                builder.AppendLine($"{data.Sl}, {data.ChangeType}, {data.ChangeItemName},{data.OldValue}, {data.UpdatedValue}, {data.CreatedByName}, {data.CreatedDate}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "MenuLog.csv");

        }
    }
}
