using _3dposManagaement.Database;
using _3dposManagaement.Repository.IRepository.IInventoryRepository;
using _3dposManagaement.Repository.IRepository.ILogRepository;
using _3dposManagaement.Repository.IRepository.IProductCategoryRepository;
using _3dposManagaement.Repository.IRepository.IProductRepository;
using _3dposManagaement.Repository.IRepository.IUnitTypeRepository;
using _3dposManagaement.Utility.CommonModel;
using CommonEntityModel.EntityModel;
using CommonEntityModel.ModelClass;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace _3dposManagaement.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private const string _areaName = "3DPOSRManageMent Module";
        private const string _controllerName = "Inventory";
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

        public InventoryController(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository, IAuditLogRepository auditLogRepository, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IProductCategoryRepository productCategoryRepository, IProductRepository productRepository, IUnitTypeRepository unitTypeRepository, IInventoryRepository inventoryRepository)
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
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> UnitTypeList(string SearchText = "", int pg = 1)
        {
            string actionName = "UnitTypeList";
            var path = "Inventory > UnitTypeList";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<UniteTypeViewModel> unitTypelist = new List<UniteTypeViewModel>();

            int recsCount = 0;
            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    unitTypelist = await _unitTypeRepository.GetUnitTypeListBySearchValue(SearchText, loggedInUserId, restaurantCode.ToString());
                }
                else
                {
                    unitTypelist = await _unitTypeRepository.GetUnitTypeList(loggedInUserId, restaurantCode.ToString());
                }

                if (unitTypelist != null)
                {
                    recsCount = unitTypelist.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var unitTypeData = unitTypelist.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No product unit type added yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;
                        ViewBag.Path = path;

                        UnitTypeMargeViewModel unitTypeMargeViewModel = new UnitTypeMargeViewModel();
                        unitTypeMargeViewModel.uniteTypeViewModel = unitTypeData;

                        return View(unitTypeMargeViewModel);
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

                        var unitTypeData = unitTypelist.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;
                        ViewBag.Path = path;

                        UnitTypeMargeViewModel unitTypeMargeViewModel = new UnitTypeMargeViewModel();
                        unitTypeMargeViewModel.uniteTypeViewModel = unitTypeData;

                        return View(unitTypeMargeViewModel);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    ViewBag.Path = path;

                    UnitTypeMargeViewModel unitTypeMargeViewModel = new UnitTypeMargeViewModel();
                    unitTypeMargeViewModel.uniteTypeViewModel = new List<UniteTypeViewModel>();

                    return View(unitTypeMargeViewModel);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                ViewBag.Path = path;

                UnitTypeMargeViewModel unitTypeMargeViewModel = new UnitTypeMargeViewModel();
                unitTypeMargeViewModel.uniteTypeViewModel = new List<UniteTypeViewModel>();

                return View(unitTypeMargeViewModel);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveUnitType()
        {
            string actionName = "SaveUnitType";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var data = HttpContext.Request.Form["dataModel"];
            UnitType model = JsonConvert.DeserializeObject<UnitType>(data);

            try
            {
                var isExistUniteType = await _unitTypeRepository.CheckExistUniteType(model, loggedInUserId, restaurantCode.ToString());
                if (isExistUniteType == "True")
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.UnitTypeExist.ToString());
                    return Json(new Confirmation { output = "exist" });
                }
                else if (isExistUniteType == "False")
                {
                    var unitTypeData = _unitTypeRepository.SaveUnitType(model, loggedInUserId, restaurantCode.ToString());

                    if (unitTypeData != null)
                    {
                        AuditLog auditLog = new AuditLog();

                        auditLog.AuditTime = DateTime.UtcNow;
                        auditLog.AreaName = _areaName;
                        auditLog.ControllerName = _controllerName;
                        auditLog.ActionName = actionName;
                        auditLog.AuditBy = auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        auditLog.PreviousInformation = null;
                        auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { unitTypeData });

                        if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                        {
                            ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                            return Json(new Confirmation { output = "error" });
                        }
                        else
                        {
                            return Json(new Confirmation { output = "success" });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return Json(new Confirmation { output = "error" });
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
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
        public async Task<JsonResult> GetUniteTypeById(int uniteTypeId)
        {
            string value = string.Empty;
            string actionName = "GetUniteTypeById";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var unitTypeData = await _unitTypeRepository.GetUniteTypeByID(uniteTypeId, loggedInUserId, restaurantCode.ToString());
                value = JsonConvert.SerializeObject(unitTypeData, Formatting.Indented, new JsonSerializerSettings
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
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> EditUnitType()
        {
            string actionName = "EditUnitType";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var data = HttpContext.Request.Form["dataModel"];
            UnitType model = JsonConvert.DeserializeObject<UnitType>(data);

            try
            {
                var isExistUniteType = await _unitTypeRepository.CheckExistUniteType(model, loggedInUserId, restaurantCode.ToString());
                if (isExistUniteType == "True")
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.UnitTypeExist.ToString());
                    return Json(new Confirmation { output = "exist" });
                }
                else if (isExistUniteType == "False")
                {
                    var unitTypeData = await _unitTypeRepository.GetUniteTypeByID(model.UnitTypeId, loggedInUserId, restaurantCode.ToString());
                    var result = _unitTypeRepository.EditUnitType(model, loggedInUserId, restaurantCode.ToString());

                    if (result != null)
                    {
                        AuditLog auditLog = new AuditLog();

                        auditLog.AuditTime = DateTime.UtcNow;
                        auditLog.AreaName = _areaName;
                        auditLog.ControllerName = _controllerName;
                        auditLog.ActionName = actionName;
                        auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        auditLog.PreviousInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { unitTypeData }); ;
                        auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { result });

                        if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                        {
                            ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                            return Json(new Confirmation { output = "error" });
                        }
                        else
                        {
                            return Json(new Confirmation { output = "success" });
                        }
                    }
                    return Json(new Confirmation { output = "error" });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
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
        public async Task<JsonResult> DeleteUnitType(int unitTypeId, string code)
        {
            string actionName = "DeleteUnitTypeRecord";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var isDelete = await _unitTypeRepository.RemoveUnitType(unitTypeId,code, loggedInUserId, restaurantCode.ToString());
                if (isDelete == "true")
                {
                    return Json(new Confirmation { output = "success" });
                }
                else if (isDelete == "exist")
                {
                    return Json(new Confirmation { output = "exist" });
                }
                else if (isDelete == "false")
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




        [AcceptVerbs("Get", "Post")]
        [Authorize]
        public async Task<IActionResult> ProductCategoryList(string SearchText = "", int pg = 1)
        {
            string actionName = "ProductCategoryList";
            var path = "Inventory > ProductCategoryList";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<ProductCategoryViewModel> productCategorylist = new List<ProductCategoryViewModel>();
            List<ProductCategoryViewModel> emptyProductCategorylist = new List<ProductCategoryViewModel>();

            int recsCount = 0;
            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    productCategorylist = await _productCategoryRepository.GetProductCategoryBySearchValue(SearchText, loggedInUserId, restaurantCode.ToString());
                }
                else
                {
                    productCategorylist = await _productCategoryRepository.GetProductCategory(loggedInUserId, restaurantCode.ToString());
                }

                if (productCategorylist != null)
                {
                    recsCount = productCategorylist.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var productCategoryData = productCategorylist.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No product category added yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;

                        ViewBag.Path = path;

                        ProductCategoryMargeViewModel productCategoryMargeViewModel = new ProductCategoryMargeViewModel();
                        productCategoryMargeViewModel.productCategoryViewModel = productCategoryData;

                        return View(productCategoryMargeViewModel);
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

                        var productCategoryData = productCategorylist.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;

                        ViewBag.Path = path;

                        ProductCategoryMargeViewModel productCategoryMargeViewModel = new ProductCategoryMargeViewModel();
                        productCategoryMargeViewModel.productCategoryViewModel = productCategoryData;

                        return View(productCategoryMargeViewModel);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    ViewBag.Path = path;

                    ProductCategoryMargeViewModel productCategoryMargeViewModel = new ProductCategoryMargeViewModel();
                    productCategoryMargeViewModel.productCategoryViewModel = new List<ProductCategoryViewModel>();

                    return View(productCategoryMargeViewModel);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                ViewBag.Path = path;

                ProductCategoryMargeViewModel productCategoryMargeViewModel = new ProductCategoryMargeViewModel();
                productCategoryMargeViewModel.productCategoryViewModel = new List<ProductCategoryViewModel>();

                return View(productCategoryMargeViewModel);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveProductCategory()
        {
            string actionName = "SaveProductCategory";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var data = HttpContext.Request.Form["dataModel"];
            ProductCategory model = JsonConvert.DeserializeObject<ProductCategory>(data);

            try
            {
                var isExistProductCategory = await _productCategoryRepository.CheckExistProductCategory(model, loggedInUserId, restaurantCode.ToString());
                if (isExistProductCategory == "True")
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.ProductCategoryExist.ToString());
                    return Json(new Confirmation { output = "exist" });
                }
                else if (isExistProductCategory == "False")
                {
                    var productCategoryData = _productCategoryRepository.SaveProductCategory(model, loggedInUserId, restaurantCode.ToString());

                    if (productCategoryData != null)
                    {
                        AuditLog auditLog = new AuditLog();

                        auditLog.AuditTime = DateTime.UtcNow;
                        auditLog.AreaName = _areaName;
                        auditLog.ControllerName = _controllerName;
                        auditLog.ActionName = actionName;
                        auditLog.AuditBy = auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        auditLog.PreviousInformation = null;
                        auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { productCategoryData });

                        if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                        {
                            ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                            return Json(new Confirmation { output = "error" });
                        }
                        else
                        {
                            return Json(new Confirmation { output = "success" });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return Json(new Confirmation { output = "error" });
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
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
        public async Task<JsonResult> GetProductCategoryById(int productCategoryId)
        {
            string value = string.Empty;
            string actionName = "GetProductCategoryById";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {                                                  
                var productCategoryData = await _productCategoryRepository.GetProductCategoryByID(productCategoryId, loggedInUserId, restaurantCode.ToString());
                value = JsonConvert.SerializeObject(productCategoryData, Formatting.Indented, new JsonSerializerSettings
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
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProductCategory()
        {
            string actionName = "EditProductCategory";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var data = HttpContext.Request.Form["dataModel"];
            ProductCategory model = JsonConvert.DeserializeObject<ProductCategory>(data);

            try
            {
                var isExistProductCategory = await _productCategoryRepository.CheckExistProductCategory(model, loggedInUserId, restaurantCode.ToString());
                if (isExistProductCategory == "True")
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.ProductCategoryExist.ToString());
                    return Json(new Confirmation { output = "exist" });
                }
                else if (isExistProductCategory == "False")
                {
                    var productCategoryData = await _productCategoryRepository.GetProductCategoryByID(model.ProductCategoryId, loggedInUserId, restaurantCode.ToString());
                    var result = _productCategoryRepository.EditProductCategory(model, loggedInUserId, restaurantCode.ToString());

                    if (result != null)
                    {
                        AuditLog auditLog = new AuditLog();

                        auditLog.AuditTime = DateTime.UtcNow;
                        auditLog.AreaName = _areaName;
                        auditLog.ControllerName = _controllerName;
                        auditLog.ActionName = actionName;
                        auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        auditLog.PreviousInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { productCategoryData }); ;
                        auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { result });

                        if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                        {
                            ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                            return Json(new Confirmation { output = "error" });
                        }
                        else
                        {
                            return Json(new Confirmation { output = "success" });
                        }
                    }
                    return Json(new Confirmation { output = "error" });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
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
        public async Task<JsonResult> DeleteProductCategory(int productCategoryId, string code)
        {
            string actionName = "DeleteProductCategory";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var isDelete = await _productCategoryRepository.RemoveProductCategory(productCategoryId, code, loggedInUserId, restaurantCode.ToString());
                if (isDelete == "true")
                {
                    return Json(new Confirmation { output = "success" });
                }
                else if (isDelete == "exist")
                {
                    return Json(new Confirmation { output = "exist" });
                }
                else if (isDelete == "false")
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




        //[AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> ProductList(string SearchText = "", int pg = 1)
        {
            string actionName = "ProductList";
            var path = "Inventory > ProductList";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<ProductModel> productlist = new List<ProductModel>();
            List<ProductModel> emptyProductlist = new List<ProductModel>();

            int recsCount = 0;
            try
            {
                if (SearchText != "" && SearchText != null)
                {
                    productlist = await _productRepository.GetProductListBySearchValue(SearchText, loggedInUserId, restaurantCode.ToString());
                }
                else
                {
                    productlist = await _productRepository.GetProductList(loggedInUserId, restaurantCode.ToString());
                }

                if (productlist != null)
                {
                    recsCount = productlist.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var productData = productlist.Skip(0).Take(search_Pager.PageSize).ToList();

                        List<UnitType> unitTypeList = await _productRepository.UnitTypeList(loggedInUserId, restaurantCode);
                        List<ProductCategory> productCategoryList = await _productRepository.ProductCategoryList(loggedInUserId, restaurantCode);
                        
                        this.ViewBag.UnitTypeList = new SelectList(unitTypeList, "UnitTypeId", "UnitTypeName");
                        this.ViewBag.ProductCategoryList = new SelectList(productCategoryList, "ProductCategoryId", "ProductCategoryName");
                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.Message = "No product added yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;
                        this.ViewBag.Path = path;

                        ProductMargeViewModel productMargeViewModel = new ProductMargeViewModel();
                        productMargeViewModel.productModels = productData;

                        return View(productMargeViewModel);
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

                        var productData = productlist.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        List<UnitType> unitTypeList = await _productRepository.UnitTypeList(loggedInUserId, restaurantCode);
                        List<ProductCategory> productCategoryList = await _productRepository.ProductCategoryList(loggedInUserId, restaurantCode);

                        this.ViewBag.UnitTypeList = new SelectList(unitTypeList, "UnitTypeId", "UnitTypeName");
                        this.ViewBag.ProductCategoryList = new SelectList(productCategoryList, "ProductCategoryId", "ProductCategoryName");
                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;
                        this.ViewBag.Path = path;

                        ProductMargeViewModel productMargeViewModel = new ProductMargeViewModel();
                        productMargeViewModel.productModels = productData;

                        return View(productMargeViewModel);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    List<UnitType> unitTypeList = await _productRepository.UnitTypeList(loggedInUserId, restaurantCode);
                    List<ProductCategory> productCategoryList = await _productRepository.ProductCategoryList(loggedInUserId, restaurantCode);

                    this.ViewBag.UnitTypeList = new SelectList(unitTypeList, "UnitTypeId", "UnitTypeName");
                    this.ViewBag.ProductCategoryList = new SelectList(productCategoryList, "ProductCategoryId", "ProductCategoryName");
                    this.ViewBag.Path = path;

                    ProductMargeViewModel productMargeViewModel = new ProductMargeViewModel();
                    productMargeViewModel.productModels = new List<ProductModel>();

                    return View(productMargeViewModel);
                }
            }
            catch (Exception ex)
            {
                List<UnitType> unitTypeList = await _productRepository.UnitTypeList(loggedInUserId, restaurantCode);
                List<ProductCategory> productCategoryList = await _productRepository.ProductCategoryList(loggedInUserId, restaurantCode);

                this.ViewBag.UnitTypeList = new SelectList(unitTypeList, "UnitTypeId", "UnitTypeName");
                this.ViewBag.ProductCategoryList = new SelectList(productCategoryList, "ProductCategoryId", "ProductCategoryName");
                this.ViewBag.Path = path;

                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View();
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveProduct()
        {
            string actionName = "SaveProduct";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var data = HttpContext.Request.Form["dataModel"];
            Product model = JsonConvert.DeserializeObject<Product>(data);

            try
            {
                var isExistProduct = await _productRepository.CheckExistProduct(model, loggedInUserId, restaurantCode.ToString());
                if (isExistProduct == "True")
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.ProductCategoryExist.ToString());
                    return Json(new Confirmation { output = "exist" });
                }
                else if (isExistProduct == "False")
                {
                    var productData = _productRepository.SaveProduct(model, loggedInUserId, restaurantCode.ToString());

                    if (productData != null)
                    {
                        AuditLog auditLog = new AuditLog();

                        auditLog.AuditTime = DateTime.UtcNow;
                        auditLog.AreaName = _areaName;
                        auditLog.ControllerName = _controllerName;
                        auditLog.ActionName = actionName;
                        auditLog.AuditBy = auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        auditLog.PreviousInformation = null;
                        auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { productData });

                        if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                        {
                            ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                            return Json(new Confirmation { output = "error" });
                        }
                        else
                        {
                            return Json(new Confirmation { output = "success" });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return Json(new Confirmation { output = "error" });
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
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

        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<IActionResult> EditProduct(int id)
        //{
        //    string actionName = "EditProduct";
        //    var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
        //    var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //    Product product = new Product();

        //    try
        //    {
        //        var unitTypeList = _DBContext.UnitType.Where(x => x.RestaurantCode == restaurantCode).OrderBy(r => r.UnitTypeName).ToList().Select(rr =>
        //                    new SelectListItem
        //                    {
        //                        Value = rr.UnitTypeId.ToString(),
        //                        Text = rr.UnitTypeName
        //                    }).ToList();

        //        var productCategoryList = _DBContext.ProductCategory.Where(x => x.RestaurantCode == restaurantCode).OrderBy(r => r.ProductCategoryName).ToList().Select(rr =>
        //                        new SelectListItem
        //                        {
        //                            Value = rr.ProductCategoryId.ToString(),
        //                            Text = rr.ProductCategoryName
        //                        }).ToList();

        //        var isExistProduct = await _productRepository.CheckExistProduct(product, loggedInUserId, restaurantCode.ToString());
        //        if (isExistProduct == "True")
        //        {
        //            ViewBag.UnitTypes = unitTypeList;
        //            ViewBag.ProductCategorys = productCategoryList;
        //            ModelState.AddModelError(string.Empty, _responseMessage.ProductExist.ToString());
        //            return View(product);
        //        }
        //        else if (isExistProduct == "False")
        //        {
        //            ViewBag.UnitTypes = unitTypeList;
        //            ViewBag.ProductCategorys = productCategoryList;

        //            var productData = await _productRepository.GetProductByID(id, loggedInUserId, restaurantCode.ToString());

        //            if (productData == null)
        //            {
        //                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
        //                return View(product);
        //            }
        //            else
        //            {
        //                return View(productData);
        //            }
        //        }
        //        else
        //        {
        //            ViewBag.UnitTypes = unitTypeList;
        //            ViewBag.ProductCategorys = productCategoryList;
        //            ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
        //            return View(product);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
        //        return View();
        //    }
        //}

        ////[AllowAnonymous]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditProduct(Product product)
        //{
        //    string actionName = "EditProduct";
        //    var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
        //    var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

        //    try
        //    {
        //        var unitTypeList = _DBContext.UnitType.Where(x => x.RestaurantCode == restaurantCode).OrderBy(r => r.UnitTypeName).ToList().Select(rr =>
        //                    new SelectListItem
        //                    {
        //                        Value = rr.UnitTypeId.ToString(),
        //                        Text = rr.UnitTypeName
        //                    }).ToList();

        //        var productCategoryList = _DBContext.ProductCategory.Where(x => x.RestaurantCode == restaurantCode).OrderBy(r => r.ProductCategoryName).ToList().Select(rr =>
        //                        new SelectListItem
        //                        {
        //                            Value = rr.ProductCategoryId.ToString(),
        //                            Text = rr.ProductCategoryName
        //                        }).ToList();

        //        ViewBag.UnitTypes = unitTypeList;
        //        ViewBag.ProductCategorys = productCategoryList;

        //        var productData = await _productRepository.GetProductByID(product.ProductId, loggedInUserId, restaurantCode.ToString());
        //        var result = _productRepository.EditProduct(product, loggedInUserId, restaurantCode.ToString());

        //        if (result != null)
        //        {
        //            AuditLog auditLog = new AuditLog();

        //            auditLog.AuditTime = DateTime.UtcNow;
        //            auditLog.AreaName = _areaName;
        //            auditLog.ControllerName = _controllerName;
        //            auditLog.ActionName = actionName;
        //            auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //            auditLog.PreviousInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { productData }); ;
        //            auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { result });

        //            if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
        //            {
        //                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
        //                return View(product);
        //            }
        //            else
        //            {
        //                return RedirectToAction("ProductList");
        //            }
        //        }
        //        return View(product);
        //    }
        //    catch (Exception ex)
        //    {
        //        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
        //        return View(product);
        //    }
        //}

        [HttpGet]
        public async Task<JsonResult> GetProductById(int productId)
        {
            string value = string.Empty;
            string actionName = "GetProductById";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var productData = await _productRepository.GetProductByID(productId, loggedInUserId, restaurantCode.ToString());
                value = JsonConvert.SerializeObject(productData, Formatting.Indented, new JsonSerializerSettings
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
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct()
        {
            string actionName = "EditProduct";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var data = HttpContext.Request.Form["dataModel"];
            Product model = JsonConvert.DeserializeObject<Product>(data);

            try
            {
                //var isExistProduct = await _productRepository.CheckExistProduct(model, loggedInUserId, restaurantCode.ToString());
                //if (isExistProduct == "True")
                //{
                //    ModelState.AddModelError(string.Empty, _responseMessage.ProductCategoryExist.ToString());
                //    return Json(new Confirmation { output = "exist" });
                //}

                var productData = await _productRepository.GetProductByID(model.ProductId, loggedInUserId, restaurantCode.ToString());
                var result = _productRepository.EditProduct(model, loggedInUserId, restaurantCode.ToString());

                if (result != null)
                {
                    AuditLog auditLog = new AuditLog();

                    auditLog.AuditTime = DateTime.UtcNow;
                    auditLog.AreaName = _areaName;
                    auditLog.ControllerName = _controllerName;
                    auditLog.ActionName = actionName;
                    auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    auditLog.PreviousInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { productData }); ;
                    auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { result });

                    if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                        return Json(new Confirmation { output = "error" });
                    }
                    else
                    {
                        return Json(new Confirmation { output = "success" });
                    }
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
        public async Task<JsonResult> DeleteProduct(int productId, string code)
        {
            string actionName = "DeleteProduct";

            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var isDelete = await _productRepository.RemoveProduct(productId, code, loggedInUserId, restaurantCode.ToString());
                if (isDelete == "true")
                {
                    return Json(new Confirmation { output = "success" });
                }
                else if (isDelete == "false")
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




        //[AllowAnonymous]
        //[HttpGet]
        //public IActionResult AddInventory()
        //{
        //    var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x=>x.Value).FirstOrDefault();
        //    var inventoryList = _DBContext.Product.Where(x => x.RestaurantCode == restaurantCode).OrderBy(r => r.ProductId).ToList().Select(rr =>
        //                    new SelectListItem
        //                    {
        //                        Value = rr.ProductId.ToString(),
        //                        Text = rr.ProductName
        //                    }).ToList();

        //    ViewBag.Inventory = inventoryList;

        //    return View();
        //}

        ////[AllowAnonymous]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult AddInventory(InventoryModel inventory)
        //{
        //    string actionName = "AddInventory";
        //    var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x=>x.Value).FirstOrDefault();
        //    var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

        //    var inventoryList = _DBContext.Product.Where(x => x.RestaurantCode == restaurantCode).OrderBy(r => r.ProductId).ToList().Select(rr =>
        //                    new SelectListItem
        //                    {
        //                        Value = rr.ProductId.ToString(),
        //                        Text = rr.ProductName
        //                    }).ToList();

        //    try
        //    {
        //        var inventoryData = _inventoryRepository.SaveInventory(inventory, loggedInUserId, restaurantCode);

        //        if (inventoryData != null)
        //        {
        //            AuditLog auditLog = new AuditLog();

        //            auditLog.AuditTime = DateTime.UtcNow;
        //            auditLog.AreaName = _areaName;
        //            auditLog.ControllerName = _controllerName;
        //            auditLog.ActionName = actionName;
        //            auditLog.AuditBy = auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //            auditLog.PreviousInformation = null;
        //            auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { inventoryData });

        //            if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
        //            {
        //                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());

        //                ViewBag.Inventory = inventoryList;
        //                return View(inventory);
        //            }
        //            else
        //            {
        //                return RedirectToAction("InventoryList");
        //            }
        //        }
        //        else
        //        {
        //            ViewBag.Inventory = inventoryList;

        //            ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
        //            return View(inventory);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());

        //        ViewBag.Inventory = inventoryList;
        //        return View(inventory);
        //    }
        //}

        //[AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        //public async Task<IActionResult> InventoryList(string txtInventoryId = "", string txtProductName = "", string txtSelectedDateFrom = "", string txtSelectedDateTo = "", int pg = 1)
        public async Task<IActionResult> InventoryList(string SearchText = "", int pg = 1)
        {
            string actionName = "InventoryList";
            var path = "Inventory > InventoryList";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<InventoryViewModel> inventoryList = new List<InventoryViewModel>();
            List<InventoryViewModel> emptyInventoryList = new List<InventoryViewModel>();

            int recsCount = 0;
            try
            {
                //if ((txtInventoryId == "" || txtInventoryId == null) && (txtProductName == "" || txtProductName == null) && (txtSelectedDateFrom == "" || txtSelectedDateFrom==null) && (txtSelectedDateTo == "" || txtSelectedDateTo==null))
                if (SearchText != "" && SearchText != null)
                {
                    inventoryList = await _inventoryRepository.GetInventoryListBySearchValue(SearchText, loggedInUserId, restaurantCode.ToString());
                }
                else
                {
                    inventoryList = await _inventoryRepository.GetInventoryList(loggedInUserId, restaurantCode.ToString());
                }

                if (inventoryList != null)
                {
                    recsCount = inventoryList.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        //var search_Pager = new SearchBarAndPagerForInventory(recsCount, pg, pageSize) { Controller = _controllerName, Action = actionName, SearchTxt = SearchTxt, txtSelectedDateFrom = txtSelectedDateFrom, txtSelectedDateTo = txtSelectedDateTo };
                        //var search_Pager = new SearchBarAndPagerForInventory(1, 1, 1) { Controller = _controllerName, Action = actionName, txtProductName = txtProductName, txtInventoryId = txtInventoryId, txtSelectedDateFrom = txtSelectedDateFrom, txtSelectedDateTo = txtSelectedDateTo };
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };

                        var inventoryData = inventoryList.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No inventory added yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;
                        this.ViewBag.Path = path;

                        return View(inventoryData);
                    }
                    else
                    {
                        const int pageSize = 5;
                        if (pg < 1)
                        {
                            pg = 1;
                        }

                        //var search_Pager = new SearchBarAndPagerForInventory(recsCount, pg, pageSize) { Controller = _controllerName, Action = actionName, txtProductName = txtProductName, txtInventoryId = txtInventoryId, txtSelectedDateFrom = txtSelectedDateFrom, txtSelectedDateTo = txtSelectedDateTo };
                        var search_Pager = new SearchBarAndPager(recsCount, pg, pageSize) { Controller = _controllerName, Action = actionName, SearchText = SearchText };

                        int recSkip = (pg - 1) * pageSize;

                        var inventoryData = inventoryList.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;
                        this.ViewBag.Path = path;
                        return View(inventoryData);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    this.ViewBag.Path = path;
                    return View(emptyInventoryList);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                this.ViewBag.Path = path;
                return View();
            }
        }

        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<IActionResult> EditInventory(int id)
        //{
        //    string actionName = "EditInventory";
        //    var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x=>x.Value).FirstOrDefault();
        //    var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //    InventoryModel inventory = new InventoryModel();

        //    try
        //    {
        //        var inventoryList = _DBContext.Product.Where(x => x.RestaurantCode == restaurantCode).OrderBy(r => r.ProductId).ToList().Select(rr =>
        //                     new SelectListItem
        //                     {
        //                         Value = rr.ProductId.ToString(),
        //                         Text = rr.ProductName
        //                     }).ToList();

        //        ViewBag.Inventory = inventoryList;

        //        var inventoryData = await _inventoryRepository.GetInventoryByID(id, loggedInUserId, restaurantCode.ToString());

        //        if (inventoryData == null)
        //        {
        //            ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
        //            return View(inventory);
        //        }
        //        else
        //        {
        //            return View(inventoryData);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
        //        return View();
        //    }
        //}

        //[AllowAnonymous]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditInventory(InventoryModel inventory)
        //{
        //    string actionName = "EditInventory";
        //    var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x=>x.Value).FirstOrDefault();
        //    var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

        //    try
        //    {
        //        var inventoryList = _DBContext.Product.Where(x => x.RestaurantCode == restaurantCode).OrderBy(r => r.ProductId).ToList().Select(rr =>
        //                     new SelectListItem
        //                     {
        //                         Value = rr.ProductId.ToString(),
        //                         Text = rr.ProductName
        //                     }).ToList();

        //        ViewBag.Inventory = inventoryList;

        //        var inventoryData = await _inventoryRepository.GetInventoryByID(inventory.InventoryId, loggedInUserId, restaurantCode.ToString());
        //        var result = _inventoryRepository.EditInventory(inventory, loggedInUserId, restaurantCode.ToString());

        //        if (result != null)
        //        {
        //            AuditLog auditLog = new AuditLog();

        //            auditLog.AuditTime = DateTime.UtcNow;
        //            auditLog.AreaName = _areaName;
        //            auditLog.ControllerName = _controllerName;
        //            auditLog.ActionName = actionName;
        //            auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //            auditLog.PreviousInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { inventoryData }); ;
        //            auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { result });

        //            if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
        //            {
        //                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
        //                return View(inventory);
        //            }
        //            else
        //            {
        //                return RedirectToAction("InventoryList");
        //            }
        //        }
        //        return View(inventory);
        //    }
        //    catch (Exception ex)
        //    {
        //        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
        //        return View(inventory);
        //    }
        //}

    }
}
