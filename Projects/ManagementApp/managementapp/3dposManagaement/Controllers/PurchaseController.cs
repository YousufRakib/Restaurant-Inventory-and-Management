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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;
using _3dposManagaement.Repository.IRepository.IProductPurchaseRepository;

namespace _3dposManagaement.Controllers
{
    [Authorize]
    public class PurchaseController : Controller
    {
        private const string _areaName = "3DPOSRManageMent Module";
        private const string _controllerName = "Purchase";
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ResponseMessage _responseMessage = new ResponseMessage();
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IProductPurchaseRepository _productPurchaseRepository;

        public PurchaseController(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository, IAuditLogRepository auditLogRepository, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IProductPurchaseRepository productPurchaseRepository)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._auditLogRepository = auditLogRepository;
            this._configuration = configuration;
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
            this._productPurchaseRepository = productPurchaseRepository;
        }
        //[AllowAnonymous]
        [HttpGet]
        public IActionResult CreatePurchase()
        {
            DateTime dateTime = DateTime.Now;
            var path = "Inventory > BazarList > PurchaseProduct";
            string purchaseCode = string.Concat(dateTime.Day.ToString(), dateTime.Month.ToString(), dateTime.Year.ToString(), dateTime.Minute.ToString(), dateTime.Second.ToString());

            ViewBag.thisId = purchaseCode;
            ViewBag.Path = path;

            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken] 
        public IActionResult SavePurchaseProduct([FromBody] ProductPurchaseCommonModel productPurchaseCommonModel)
        {
            string actionName = "SavePurchaseProduct";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var productPurchaseData = _productPurchaseRepository.AddPurchaseProduct(productPurchaseCommonModel, loggedInUserId, restaurantCode);

                if (productPurchaseData != null)
                {
                    AuditLog auditLog = new AuditLog();

                    auditLog.AuditTime = DateTime.UtcNow;
                    auditLog.AreaName = _areaName;
                    auditLog.ControllerName = _controllerName;
                    auditLog.ActionName = actionName;
                    auditLog.AuditBy = auditLog.AuditBy = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    auditLog.PreviousInformation = null;
                    auditLog.UpdatedInformation = Newtonsoft.Json.JsonConvert.SerializeObject(new { productPurchaseData });

                    if (!_auditLogRepository.SaveAuditLog(auditLog, loggedInUserId))
                    {
                        ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());

                        return Json(new Confirmation { output = "error", msg = "Please check Error Log or Contact with your support engineer!" });
                    }
                    else
                    {
                        return Json(new Confirmation { output = "success" });
                    }
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

        [HttpGet]
        public async Task<IActionResult> ProductPurchaseView(int pg = 1, string id = "")
        {
            string actionName = "ProductPurchaseView";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            PurchaseProductMargeModels purchaseProductMargeModels = new PurchaseProductMargeModels();
            PurchaseProductMargeModels emptypurchaseProductMargeModels = new PurchaseProductMargeModels();

            int recsCount = 0;
            try
            {
                purchaseProductMargeModels = await _productPurchaseRepository.GetProductPurchaseByPurchaseCode(loggedInUserId, restaurantCode, id);

                if (purchaseProductMargeModels != null)
                {
                    recsCount = purchaseProductMargeModels.purchaseProductViewModel.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = "" };
                        var purchaseMasterData = purchaseProductMargeModels.purchaseProductViewModel.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No product purchase in this PurchaseCode!";
                        this.ViewBag.searchBar_Pager = search_Pager;

                        PurchaseProductMargeModels purchaseProductMargeModels1 = new PurchaseProductMargeModels();
                        purchaseProductMargeModels1.purchaseProductCategoryWiseSummations = purchaseProductMargeModels.purchaseProductCategoryWiseSummations;
                        purchaseProductMargeModels1.purchaseProductViewModel = purchaseMasterData;

                        return View(purchaseProductMargeModels1);
                    }
                    else
                    {
                        const int pageSize = 5;
                        if (pg < 1)
                        {
                            pg = 1;
                        }

                        var search_Pager = new SearchBarAndPager(recsCount, pg, pageSize) { Controller = _controllerName, Action = actionName, SearchText = "" };

                        int recSkip = (pg - 1) * pageSize;

                        var purchaseMasterData = purchaseProductMargeModels.purchaseProductViewModel.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;

                        PurchaseProductMargeModels purchaseProductMargeModels1 = new PurchaseProductMargeModels();
                        purchaseProductMargeModels1.purchaseProductCategoryWiseSummations = purchaseProductMargeModels.purchaseProductCategoryWiseSummations;
                        purchaseProductMargeModels1.purchaseProductViewModel = purchaseMasterData;
                        purchaseProductMargeModels1.NetTotal = purchaseProductMargeModels.NetTotal.ToString().Substring(0, purchaseProductMargeModels.NetTotal.Length - 3);

                        return View(purchaseProductMargeModels1);
                    }
                }
                else
                {
                    emptypurchaseProductMargeModels.purchaseProductViewModel = new List<PurchaseProductViewModel>();
                    emptypurchaseProductMargeModels.purchaseProductCategoryWiseSummations = new List<PurchaseProductCategoryWiseSummation>();
                    //ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(emptypurchaseProductMargeModels);
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                return View();
            }
        }

        public async Task<IActionResult> GetProductCategory()
        {
            string actionName = "GetProductCategory";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var productCategoryList = await _productPurchaseRepository.ProductCategoryList(loggedInUserId, restaurantCode.ToString());

                var jsonResult = Json(productCategoryList);
                return jsonResult;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return Json(new Confirmation { output = "error", msg = "Please check Error Log or Contact with your support engineer!" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductInfo(int productCategoryId)
        {
            string actionName = "GetProductInfo";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var productInfo = await _productPurchaseRepository.ProductInfoByProductCategoryId(loggedInUserId, restaurantCode.ToString(), productCategoryId);

                var jsonResult = Json(productInfo);
                return jsonResult;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return Json(new Confirmation { output = "error", msg = "Please check Error Log or Contact with your support engineer!" });
            }
        }

        public async Task<IActionResult> GetProductByProductCategoryId(int productCategoryId)
        {
            string actionName = "GetProductByProductCategoryId";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var productList = await _productPurchaseRepository.ProductList(loggedInUserId, restaurantCode.ToString(), productCategoryId);

                var jsonResult = Json(productList);
                return jsonResult;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return Json(new Confirmation { output = "error", msg = "Please check Error Log or Contact with your support engineer!" });
            }
        }

        public async Task<IActionResult> GetUnitTypeByProductId(int productId)
        {
            string actionName = "GetUnitTypeByProductId";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var unitTypeData = await _productPurchaseRepository.UniteTypeList(loggedInUserId, restaurantCode.ToString(), productId);

                //var unitTypeList = _DBContext.Product
                //    .Join(_DBContext.UnitType, P => P.UnitTypeId, U => U.UnitTypeId, (P, U) => new { P, U })
                //    .Where(x => x.P.RestaurantCode == restaurantCode && x.P.ProductId == productId)
                //    .Select(x => new
                //    {
                //        x.U.UnitTypeId,
                //        x.U.UnitTypeName
                //    }).FirstOrDefault();

                return Json(new Confirmation { output = "success", returnvalue = unitTypeData });

            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return Json(new Confirmation { output = "error", msg = "Please check Error Log or Contact with your support engineer!" });
            }
        }

        //[AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> PurchaseMasterList(string SearchText = "", string txtSelectedDateFrom = "", string txtSelectedDateTo = "", int pg = 1)
        {
            string actionName = "PurchaseMasterList";
            var path = "Inventory > BazarList";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            PurchaseProductMargeModelsForMaster purchaseProductMargeModelsForMaster = new PurchaseProductMargeModelsForMaster();
            PurchaseProductMargeModelsForMaster emptypurchaseProductMargeModelsForMaster = new PurchaseProductMargeModelsForMaster();

            int recsCount = 0;
            try
            {
                if ((SearchText == "" || SearchText == null) && (txtSelectedDateFrom == "" || txtSelectedDateFrom == null) && (txtSelectedDateTo == "" || txtSelectedDateTo == null))
                {
                    purchaseProductMargeModelsForMaster = await _productPurchaseRepository.GetProductPurchaseMasterList(loggedInUserId, restaurantCode.ToString());

                }
                else
                {
                    purchaseProductMargeModelsForMaster = await _productPurchaseRepository.GetProductPurchaseMasterListBySearchValue(SearchText, txtSelectedDateFrom, txtSelectedDateTo, loggedInUserId, restaurantCode.ToString());
                }

                if (purchaseProductMargeModelsForMaster != null)
                {
                    recsCount = purchaseProductMargeModelsForMaster.purchaseMasterModel.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        var search_Pager = new SearchBarAndPagerForPurchaseProductMaster(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText, txtSelectedDateFrom = txtSelectedDateFrom, txtSelectedDateTo = txtSelectedDateTo };
                        var purchaseMasterData = purchaseProductMargeModelsForMaster.purchaseMasterModel.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No product purchase yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;
                        ViewBag.Path = path;

                        PurchaseProductMargeModelsForMaster purchaseProductMargeModelsForMaster1 = new PurchaseProductMargeModelsForMaster();
                        purchaseProductMargeModelsForMaster1.purchaseProductCategoryWiseSummations = purchaseProductMargeModelsForMaster.purchaseProductCategoryWiseSummations;
                        purchaseProductMargeModelsForMaster1.purchaseMasterModel = purchaseMasterData;

                        return View(purchaseProductMargeModelsForMaster1);
                    }
                    else
                    {
                        const int pageSize = 5;
                        if (pg < 1)
                        {
                            pg = 1;
                        }

                        var search_Pager = new SearchBarAndPagerForPurchaseProductMaster(recsCount, pg, pageSize) { Controller = _controllerName, Action = actionName, SearchText = SearchText, txtSelectedDateFrom = txtSelectedDateFrom, txtSelectedDateTo = txtSelectedDateTo };

                        int recSkip = (pg - 1) * pageSize;

                        var purchaseMasterData = purchaseProductMargeModelsForMaster.purchaseMasterModel.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;
                        ViewBag.Path = path;

                        PurchaseProductMargeModelsForMaster purchaseProductMargeModelsForMaster1 = new PurchaseProductMargeModelsForMaster();
                        purchaseProductMargeModelsForMaster1.purchaseProductCategoryWiseSummations = purchaseProductMargeModelsForMaster.purchaseProductCategoryWiseSummations;
                        purchaseProductMargeModelsForMaster1.purchaseMasterModel = purchaseMasterData;
                        purchaseProductMargeModelsForMaster1.NetTotal = purchaseProductMargeModelsForMaster.NetTotal.ToString().Substring(0, purchaseProductMargeModelsForMaster.NetTotal.Length - 3);

                        return View(purchaseProductMargeModelsForMaster1);
                    }
                }
                else
                {
                    emptypurchaseProductMargeModelsForMaster.purchaseMasterModel = new List<PurchaseMasterModel>();
                    emptypurchaseProductMargeModelsForMaster.purchaseProductCategoryWiseSummations = new List<PurchaseProductCategoryWiseSummation>();
                    //ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    ViewBag.Path = path;
                    return View(emptypurchaseProductMargeModelsForMaster);
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

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> PurchasedProductList(string txtPurchaseCode = "", string txtProductCategoryName = "", string txtProductName = "", string txtUniteTypeName = "", string txtSelectedDateFrom = "", string txtSelectedDateTo = "", int pg = 1)
        {
            string actionName = "PurchasedProductList";
            var restaurantCode = User.Claims.Where(x => x.Type == "RestaurantCode").Select(x => x.Value).FirstOrDefault();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) == null ? 0 : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<PurchaseProductViewModel> purchaseProductList = new List<PurchaseProductViewModel>();
            List<PurchaseProductViewModel> emptyPurchaseProductList = new List<PurchaseProductViewModel>();

            int recsCount = 0;
            try
            {
                if ((txtPurchaseCode == "" || txtPurchaseCode == null) && (txtProductCategoryName == "" || txtProductCategoryName == null) && (txtProductName == "" || txtProductName == null) && (txtUniteTypeName == "" || txtUniteTypeName == null) && (txtSelectedDateFrom == "" || txtSelectedDateFrom == null) && (txtSelectedDateTo == "" || txtSelectedDateTo == null))
                {
                    purchaseProductList = await _productPurchaseRepository.GetRestaurantWiseAllPurchaseList(loggedInUserId, restaurantCode.ToString());
                }
                else
                {
                    purchaseProductList = await _productPurchaseRepository.GetRestaurantWiseAllPurchaseListBySearchValue(txtPurchaseCode, txtProductCategoryName, txtProductName, txtUniteTypeName, txtSelectedDateFrom, txtSelectedDateTo, loggedInUserId, restaurantCode.ToString());
                }

                if (purchaseProductList != null)
                {
                    recsCount = purchaseProductList.Count();

                    if (recsCount == 0 || recsCount == null)
                    {
                        //var search_Pager = new SearchBarAndPager(1, 1, 1) { Controller = _controllerName, Action = actionName, SearchText = SearchText };
                        var search_Pager = new SearchBarAndPagerForProductPurchase(1, 1, 1) { Controller = _controllerName, Action = actionName, txtPurchaseCode = txtPurchaseCode, txtProductCategoryName = txtProductCategoryName, txtProductName = txtProductName, txtUniteTypeName = txtUniteTypeName, txtSelectedDateFrom = txtSelectedDateFrom, txtSelectedDateTo = txtSelectedDateTo };
                        var purchaseMasterData = purchaseProductList.Skip(0).Take(search_Pager.PageSize).ToList();

                        ViewBag.Message = "No product purchase yet!";
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(purchaseMasterData);
                    }
                    else
                    {
                        const int pageSize = 5;
                        if (pg < 1)
                        {
                            pg = 1;
                        }

                        var search_Pager = new SearchBarAndPagerForProductPurchase(recsCount, pg, pageSize) { Controller = _controllerName, Action = actionName, txtPurchaseCode = txtPurchaseCode, txtProductCategoryName = txtProductCategoryName, txtProductName = txtProductName, txtUniteTypeName = txtUniteTypeName, txtSelectedDateFrom = txtSelectedDateFrom, txtSelectedDateTo = txtSelectedDateTo };
                        //var search_Pager = new SearchBarAndPager(recsCount, pg, pageSize) { Controller = _controllerName, Action = actionName, SearchText = SearchText };

                        int recSkip = (pg - 1) * pageSize;

                        var purchaseMasterData = purchaseProductList.Skip(recSkip).Take(search_Pager.PageSize).ToList();

                        this.ViewBag.loggedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        this.ViewBag.searchBar_Pager = search_Pager;

                        return View(purchaseMasterData);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _responseMessage.TryCatchError.ToString());
                    return View(emptyPurchaseProductList);
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
