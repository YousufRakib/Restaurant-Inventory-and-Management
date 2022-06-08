using _3dposManagaement.Repository.IRepository.IProductPurchaseRepository;
using _3dposManagaement.Database;
using _3dposManagaement.Repository.IRepository.ILogRepository;
using _3dposManagaement.Repository.IRepository.IProductRepository;
using _3dposManagaement.Utility.CommonModel;
using CommonEntityModel.EntityModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Dapper;

namespace _3dposManagaement.Repository.Repository.ProductPurchaseRepository
{
    public class ProductPurchaseRepository : IProductPurchaseRepository
    {
        private const string _areaName = "3DPOSManagement Module";
        private const string _controllerName = "ProductPurchase";
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IConfiguration _configuration;

        public ProductPurchaseRepository(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository, IConfiguration configuration)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._configuration = configuration;
        }

        public ProductPurchaseCommonModel AddPurchaseProduct(ProductPurchaseCommonModel productPurchaseCommonModel, int loggedInUserId, string restaurantCode)
        {
            string actionName = "AddPurchaseProduct";
            try
            {
                using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();

                        ProductPurchaseMaster productPurchaseMaster = new ProductPurchaseMaster();
                        productPurchaseMaster.PurchaseCode = productPurchaseCommonModel.PurchaseMasterModel.PurchaseCode;
                        productPurchaseMaster.Comment = productPurchaseCommonModel.PurchaseMasterModel.Comment;
                        productPurchaseMaster.RestaurantCode = restaurantCode;
                        productPurchaseMaster.CreatedByName = userData.UserName;
                        productPurchaseMaster.CreatedBy = loggedInUserId;
                        productPurchaseMaster.CreatedDate = DateTime.UtcNow;

                        _DBContext.ProductPurchaseMaster.Add(productPurchaseMaster);
                        _DBContext.SaveChanges();

                        foreach (var data in productPurchaseCommonModel.PurchaseProductSaveModel)
                        {
                            ProductPurchase productPurchase = new ProductPurchase();

                            productPurchase.PurchaseCode = data.PurchaseCode;
                            productPurchase.RestaurantCode = restaurantCode;
                            productPurchase.ProductId = data.ProductId;
                            productPurchase.ProductCategoryId = data.ProductCategoryId;
                            productPurchase.UnitTypeId = data.UnitTypeId;
                            productPurchase.Price = data.UnitPrice;
                            productPurchase.Quantity = data.Quantity;
                            productPurchase.Total = data.Total;
                            productPurchase.PurchaseFrom = data.PurchasedFrom;
                            productPurchase.CreatedByName = userData.UserName;
                            productPurchase.CreatedBy = loggedInUserId;
                            productPurchase.CreatedDate = DateTime.UtcNow;
                            _DBContext.ProductPurchase.Add(productPurchase);
                            _DBContext.SaveChanges();

                            var inventoryData = _DBContext.InventoryModel.Where(x => x.ProductId == productPurchase.ProductId/* && x.RestaurantCode == restaurantCode*/).OrderByDescending(x => x.ProductId).Take(1).FirstOrDefault();

                            if (inventoryData != null)
                            {
                                inventoryData.UpdatedBy = loggedInUserId;
                                inventoryData.UpdatedDate = DateTime.UtcNow;
                                _DBContext.SaveChanges();

                                var inventoryLockedData = _DBContext.InventoryModel.Where(x => x.ProductId == productPurchase.ProductId/* && x.RestaurantCode == restaurantCode*/).OrderByDescending(x => x.ProductId).Take(1).FirstOrDefault();

                                inventoryLockedData.Price = inventoryData.Price == null ? 0 + data.UnitPrice : inventoryData.Price + data.UnitPrice;
                                inventoryLockedData.Quantity = inventoryData.Quantity == null ? 0 + data.Quantity : inventoryData.Quantity + data.Quantity;
                                inventoryLockedData.Total = inventoryData.Total == null ? 0 + data.Total : inventoryData.Total + data.Total;
                                inventoryLockedData.UpdatedByName = userData.UserName;
                                inventoryLockedData.UpdatedBy = loggedInUserId;
                                inventoryLockedData.UpdatedDate = DateTime.UtcNow;
                                _DBContext.SaveChanges();
                            }
                            else
                            {
                                InventoryModel inventoryModel = new InventoryModel();
                                inventoryModel.ProductId = data.ProductId;
                                inventoryModel.Price = data.UnitPrice;
                                inventoryModel.Quantity = data.Quantity;
                                inventoryModel.Total = data.Total;
                                inventoryModel.CreatedByName = userData.UserName;
                                inventoryModel.CreatedBy = loggedInUserId;
                                inventoryModel.CreatedDate = DateTime.UtcNow;
                                _DBContext.InventoryModel.Add(inventoryModel);
                                _DBContext.SaveChanges();
                            }

                            //if (HasUnsavedChanges() == true)
                            //{
                            //    var inventoryDatas = _DBContext.InventoryModel.Where(x => x.ProductId == productPurchase.ProductId/* && x.RestaurantCode == restaurantCode*/).OrderByDescending(x => x.ProductId).Take(1).FirstOrDefault();

                            //    if (inventoryDatas != null)
                            //    {
                            //        inventoryDatas.Price = inventoryDatas.Price == null ? 0 + data.UnitPrice : inventoryDatas.Price + data.UnitPrice;
                            //        inventoryDatas.Quantity = inventoryDatas.Quantity == null ? 0 + data.Quantity : inventoryDatas.Quantity + data.Quantity;
                            //        inventoryDatas.Total = inventoryDatas.Total == null ? 0 + data.Total : inventoryDatas.Total + data.Total;
                            //        inventoryDatas.UpdatedByName = userData.UserName;
                            //        inventoryDatas.UpdatedBy = loggedInUserId;
                            //        inventoryDatas.UpdatedDate = DateTime.UtcNow;
                            //    }
                            //    else
                            //    {
                            //        InventoryModel inventoryModel = new InventoryModel();

                            //        inventoryModel.Price = data.UnitPrice;
                            //        inventoryModel.Quantity = data.Quantity;
                            //        inventoryModel.Total = data.Total;
                            //        inventoryModel.CreatedByName = userData.UserName;
                            //        inventoryModel.CreatedBy = loggedInUserId;
                            //        inventoryModel.CreatedDate = DateTime.UtcNow;
                            //    }
                            //    _DBContext.SaveChanges();
                            //}
                            //else
                            //{
                            //    _DBContext.SaveChanges();
                            //}
                            //_DBContext.SaveChanges();
                        }
                        transaction.Commit();
                        return productPurchaseCommonModel;
                    }
                    catch (Exception ex)
                    {
                        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                        transaction.Rollback();
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public bool HasUnsavedChanges()
        {
            return _DBContext.ChangeTracker.Entries().Any(e => e.State == EntityState.Added
                                                      || e.State == EntityState.Modified
                                                      || e.State == EntityState.Deleted);
        }

        public int? GetProductPurchaseMasterMaxId(int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetProductPurchaseMasterList";
            try
            {
                int purchaseMaxId = Convert.ToInt32((_DBContext.ProductPurchaseMaster.Select(x => (long?)x.PurchaseMasterId).Max() ?? 0) + 1);
                return purchaseMaxId;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<PurchaseProductMargeModels> GetProductPurchaseByPurchaseCode(int loggedInUserId, string restaurantCode, string purchaseCode)
        {
            string actionName = "GetProductPurchaseByPurchaseCode";
            try
            {
                PurchaseProductMargeModels purchaseProductMargeModels = new PurchaseProductMargeModels();


                string sqlForPurchaseList = $@"Select ROW_NUMBER() Over (Order by PP.PurchaseId desc) As Sl, PP.PurchaseId,PP.PurchaseCode,PP.Price,PP.Quantity,PP.Total,P.ProductName,PC.ProductCategoryName,UT.UnitTypeName,PP.PurchaseFrom,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, PP.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate from ProductPurchase as PP
                                            left join Product as P on PP.ProductId=P.ProductId
                                            left join ProductCategory as PC on PP.ProductCategoryId=PC.ProductCategoryId
                                            left join UnitType as UT on PP.UnitTypeId=UT.UnitTypeId
                                            Where PP.RestaurantCode=@RestaurantCode and PP.PurchaseCode=@PurchaseCode order by PP.PurchaseId desc";

                string sqlForCategoryWiseSummation = $@"Select PC.ProductCategoryName,SUM(PP.Total) as TotalPrice from ProductPurchase as PP
                                                        left join ProductCategory as PC on PP.ProductCategoryId=PC.ProductCategoryId
                                                        Where PP.RestaurantCode=@RestaurantCode and PP.PurchaseCode=@PurchaseCode
                                                        group by PC.ProductCategoryName,PP.PurchaseCode";

                //string connString = _configuration.GetConnectionString("RestaurantDBConnection");
                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var purchaseList = connection.Query<PurchaseProductViewModel>(sqlForPurchaseList, new { RestaurantCode = restaurantCode, PurchaseCode = purchaseCode }).ToList();

                    var purchaseSummation = connection.Query<PurchaseProductCategoryWiseSummation>(sqlForCategoryWiseSummation, new { RestaurantCode = restaurantCode, PurchaseCode = purchaseCode }).ToList();

                    purchaseProductMargeModels.purchaseProductCategoryWiseSummations = purchaseSummation;
                    purchaseProductMargeModels.purchaseProductViewModel = purchaseList;

                    decimal sum = 0;
                    if (purchaseSummation.Count() != 0)
                    {
                        foreach (var data in purchaseSummation)
                        {
                            sum = sum + data.TotalPrice;
                        }
                    }
                    purchaseProductMargeModels.NetTotal = sum.ToString();

                    return purchaseProductMargeModels;
                }

            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<PurchaseProductViewModel>> GetRestaurantWiseAllPurchaseList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetRestaurantWiseAllPurchaseList";
            try
            {
                List<PurchaseProductViewModel> purchaseProductViewModel = new List<PurchaseProductViewModel>();

                string sqlForPurchaseList = $@"Select ROW_NUMBER() Over (Order by PP.PurchaseId desc) As Sl, PP.PurchaseId,PP.PurchaseCode,PP.Price,PP.Quantity,PP.Total,P.ProductName,PC.ProductCategoryName,UT.UnitTypeName,PP.PurchaseFrom,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, PP.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate  from ProductPurchase as PP
                                left join Product as P on PP.ProductId=P.ProductId
                                left join ProductCategory as PC on PP.ProductCategoryId=PC.ProductCategoryId
                                left join UnitType as UT on PP.UnitTypeId=UT.UnitTypeId
                                Where PP.RestaurantCode=@RestaurantCode order by PP.PurchaseId desc";

                //string connString = _configuration.GetConnectionString("RestaurantDBConnection");
                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var purchaseList = connection.Query<PurchaseProductViewModel>(sqlForPurchaseList, new { RestaurantCode = restaurantCode }).ToList();

                    return purchaseList;
                }

            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<PurchaseProductViewModel>> GetRestaurantWiseAllPurchaseListBySearchValue(string txtPurchaseCode, string txtProductCategoryName, string txtProductName, string txtUniteTypeName, string txtSelectedDateFrom, string txtSelectedDateTo, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetRestaurantWiseAllPurchaseListBySearchValue";
            try
            {
                txtSelectedDateFrom = Convert.ToDateTime(txtSelectedDateFrom).ToString("MM/dd/yyyy");
                txtSelectedDateTo = Convert.ToDateTime(txtSelectedDateTo).ToString("MM/dd/yyyy");

                List<PurchaseProductViewModel> purchaseProductViewModelList = new List<PurchaseProductViewModel>();
                string purchaseCodeQuery = " ";
                string productCategoryNameQuery = " ";
                string productNameQuery = " ";
                string uniteTypeNameQuery = " ";
                string dateQuery = " ";

                // bool isInt = txtInventoryId.All(char.IsDigit);

                if (txtPurchaseCode != null && txtPurchaseCode != "")
                {
                    purchaseCodeQuery = " AND PP.PurchaseCode=" + txtPurchaseCode + "";
                }
                if (txtProductCategoryName != null && txtProductCategoryName != "")
                {
                    productCategoryNameQuery = " And PC.ProductCategoryName like '%" + txtProductCategoryName + "%'";
                }
                if (txtProductName != null && txtProductName != "")
                {
                    productNameQuery = " And P.ProductName like '%" + txtProductName + "%'";
                }
                if (txtUniteTypeName != null && txtUniteTypeName != "")
                {
                    uniteTypeNameQuery = " And UT.UnitTypeName like '%" + txtUniteTypeName + "%'";
                }

                if (txtSelectedDateFrom != null && txtSelectedDateFrom != "" && txtSelectedDateTo != null && txtSelectedDateTo != "")
                {
                    dateQuery = "And (CONVERT(date,PP.CreatedDate)>=CONVERT(date,'" + txtSelectedDateFrom + "')) AND (CONVERT(date,PP.CreatedDate)<=CONVERT(date,'" + txtSelectedDateTo + "'))";
                }

                string sqlForPurchaseList = $@"Select ROW_NUMBER() Over (Order by PP.PurchaseId asc) As Sl, PP.PurchaseId,PP.PurchaseCode,PP.Price,PP.Quantity,PP.Total,P.ProductName,PC.ProductCategoryName,UT.UnitTypeName,PP.PurchaseFrom,PP.CreatedDate from ProductPurchase as PP
                                            left join Product as P on PP.ProductId=P.ProductId
                                            left join ProductCategory as PC on PP.ProductCategoryId=PC.ProductCategoryId
                                            left join UnitType as UT on PP.UnitTypeId=UT.UnitTypeId
                                            Where PP.RestaurantCode=@RestaurantCode @PurchaseCodeQuery @ProductCategoryNameQuery @ProductNameQuery @UniteTypeNameQuery @DateQuery order by PP.PurchaseId desc";


                //string connString = _configuration.GetConnectionString("RestaurantDBConnection");
                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<PurchaseProductViewModel>(sqlForPurchaseList, new { RestaurantCode = restaurantCode, PurchaseCodeQuery = purchaseCodeQuery, ProductCategoryNameQuery = productCategoryNameQuery, ProductNameQuery = productNameQuery, UniteTypeNameQuery = uniteTypeNameQuery, DateQuery = dateQuery }).ToList();

                    return query;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<PurchaseProductMargeModelsForMaster> GetProductPurchaseMasterList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetProductPurchaseMasterList";
            try
            {
                PurchaseProductMargeModelsForMaster purchaseProductMargeModelsForMaster = new PurchaseProductMargeModelsForMaster();

                string sqlQuery = @"select ROW_NUMBER() Over (Order by PurchaseMasterId desc) As Sl, RestaurantCode,PurchaseMasterId,PurchaseCode,CreatedByName,UpdatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate,Comment
                                    from ProductPurchaseMaster 
                                    where RestaurantCode='" + restaurantCode + "' And CONVERT(date,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))))=CONVERT(date,GETDATE()) order by PurchaseMasterId DESC";

                string sqlForCategoryWiseSummation = $@"Select PC.ProductCategoryName,SUM(PP.Total) as TotalPrice from ProductPurchase as PP
                                                        left join ProductCategory as PC on PP.ProductCategoryId=PC.ProductCategoryId
                                                        Where PP.RestaurantCode='" + restaurantCode + "'  And  CONVERT(date,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, PP.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))))=CONVERT(date,GETDATE())  group by PC.ProductCategoryName";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<PurchaseMasterModel>(sqlQuery).ToList();

                    var purchaseSummation = connection.Query<PurchaseProductCategoryWiseSummation>(sqlForCategoryWiseSummation).ToList();

                    purchaseProductMargeModelsForMaster.purchaseMasterModel = query;
                    purchaseProductMargeModelsForMaster.purchaseProductCategoryWiseSummations = purchaseSummation;

                    decimal sum = 0;
                    if (purchaseSummation.Count() != 0)
                    {
                        foreach (var data in purchaseSummation)
                        {
                            sum = sum + data.TotalPrice;
                        }
                    }
                    purchaseProductMargeModelsForMaster.NetTotal = sum.ToString();

                    return purchaseProductMargeModelsForMaster;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<PurchaseProductMargeModelsForMaster> GetProductPurchaseMasterListBySearchValue(string SearchText, string txtSelectedDateFrom, string txtSelectedDateTo, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetProductPurchaseMasterListBySearchValue";
            try
            {
                PurchaseProductMargeModelsForMaster purchaseProductMargeModelsForMaster = new PurchaseProductMargeModelsForMaster();

                string sqlForCategoryWiseSummation = "";
                string sqlQuery = "";

                //string searchTextQuery = null;
                //string dateQuery = " ";
                //string dateQueryForSum = " ";

                //if (SearchText != null && SearchText != "")
                //{
                //    searchTextQuery = " AND PurchaseMasterId=" + SearchText + " OR PurchaseCode like '%" + SearchText + "%' OR CreatedByName like '%" + SearchText + "%' OR UpdatedByName like '%" + SearchText + "%'";
                //}

                //if (txtSelectedDateFrom != null && txtSelectedDateFrom != "" && txtSelectedDateTo != null && txtSelectedDateTo != "")
                //{
                //    dateQuery = "CONVERT(date,CreatedDate)>=CONVERT(date,'" + txtSelectedDateFrom + "') AND CONVERT(date,CreatedDate)<=CONVERT(date,'" + txtSelectedDateTo + "')";
                //    dateQueryForSum = " And (CONVERT(date,PP.CreatedDate)>=CONVERT(date,'" + txtSelectedDateFrom + "')) AND (CONVERT(date,PP.CreatedDate)<=CONVERT(date,'" + txtSelectedDateTo + "'))";
                //}

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    if (txtSelectedDateFrom != null && txtSelectedDateFrom != "" && txtSelectedDateTo != null && txtSelectedDateTo != "")
                    {
                        sqlQuery = @"select RestaurantCode,PurchaseMasterId,PurchaseCode,CreatedByName,UpdatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate,Comment from ProductPurchaseMaster where RestaurantCode=@RestaurantCode AND CONVERT(date,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))))>=CONVERT(date,@txtSelectedDateFrom) AND CONVERT(date,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))))<=CONVERT(date,@txtSelectedDateTo) order by PurchaseMasterId DESC";

                        sqlForCategoryWiseSummation = $@"Select PC.ProductCategoryName,SUM(PP.Total) as TotalPrice from ProductPurchase as PP
                                                        left join ProductCategory as PC on PP.ProductCategoryId=PC.ProductCategoryId
                                                        Where PP.RestaurantCode=@RestaurantCode And (CONVERT(date,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, PP.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))))>=CONVERT(date,@txtSelectedDateFrom)) AND (CONVERT(date,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, PP.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))))<=CONVERT(date,@txtSelectedDateTo)) group by PC.ProductCategoryName";

                        var query = connection.Query<PurchaseMasterModel>(sqlQuery, new { RestaurantCode = restaurantCode, txtSelectedDateFrom = txtSelectedDateFrom, txtSelectedDateTo = txtSelectedDateTo }).ToList();

                        var purchaseSummation = connection.Query<PurchaseProductCategoryWiseSummation>(sqlForCategoryWiseSummation, new { RestaurantCode = restaurantCode, txtSelectedDateFrom = txtSelectedDateFrom, txtSelectedDateTo = txtSelectedDateTo }).ToList();

                        purchaseProductMargeModelsForMaster.purchaseMasterModel = query;
                        purchaseProductMargeModelsForMaster.purchaseProductCategoryWiseSummations = purchaseSummation;

                        decimal sum = 0;
                        if (purchaseSummation.Count() != 0)
                        {
                            foreach (var data in purchaseSummation)
                            {
                                sum = sum + data.TotalPrice;
                            }
                        }
                        purchaseProductMargeModelsForMaster.NetTotal = sum.ToString();

                        return purchaseProductMargeModelsForMaster;
                    }
                    else
                    {
                        return purchaseProductMargeModelsForMaster;
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<ProductCategoryList>> ProductCategoryList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "ProductCategoryList";
            try
            {
                List<ProductCategoryList> _productCategoryList = new List<ProductCategoryList>();

                var productCategoryList = _DBContext.ProductCategory.Where(x => x.RestaurantCode == restaurantCode).OrderBy(r => r.ProductCategoryName)
                .Select(x => new
                {
                    x.ProductCategoryId,
                    x.ProductCategoryName
                }).ToList();

                foreach (var data in productCategoryList)
                {
                    _productCategoryList.Add(new ProductCategoryList
                    {
                        ProductCategoryId = data.ProductCategoryId,
                        ProductCategoryName = data.ProductCategoryName
                    });
                }
                return _productCategoryList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<ProductList>> ProductList(int loggedInUserId, string restaurantCode, int productCategoryId)
        {
            string actionName = "ProductList";
            try
            {
                List<ProductList> _productList = new List<ProductList>();

                var productList = _DBContext.Product.Where(x => x.RestaurantCode == restaurantCode && x.ProductCategoryId == productCategoryId).OrderBy(r => r.ProductName)
                .Select(x => new
                {
                    x.ProductId,
                    x.ProductName
                }).ToList();

                foreach (var data in productList)
                {
                    _productList.Add(new ProductList
                    {
                        ProductId = data.ProductId,
                        ProductName = data.ProductName
                    });
                }
                return _productList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<UniteTypeList> UniteTypeList(int loggedInUserId, string restaurantCode, int productId)
        {
            string actionName = "UniteTypeList";
            try
            {
                var unitType = _DBContext.Product
                    .Join(_DBContext.UnitType, P => P.UnitTypeId, U => U.UnitTypeId, (P, U) => new { P, U })
                    .Where(x => x.P.RestaurantCode == restaurantCode && x.P.ProductId == productId)
                    .Select(x => new
                    {
                        x.U.UnitTypeId,
                        x.U.UnitTypeName
                    }).FirstOrDefault();

                UniteTypeList uniteTypeList = new UniteTypeList();
                uniteTypeList.UnitTypeId = unitType.UnitTypeId;
                uniteTypeList.UnitTypeName = unitType.UnitTypeName;

                return uniteTypeList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }
        public async Task<ProductInfo> ProductInfoByProductCategoryId(int loggedInUserId, string restaurantCode, int productCategoryId)
        {
            string actionName = "ProductInfoByProductCategoryId";
            try
            {
                ProductInfo productInfo = new ProductInfo();

                productInfo = _DBContext.Product
                    .Join(_DBContext.UnitType, P => P.UnitTypeId, U => U.UnitTypeId, (P, U) => new { P, U })
                    .Where(x => x.P.RestaurantCode == restaurantCode && x.P.ProductCategoryId == productCategoryId).OrderBy(r => r.P.ProductName)
                    .Select(x => new ProductInfo
                    {
                        ProductId = x.P.ProductId,
                        ProductName = x.P.ProductName,
                        UnitTypeId = x.U.UnitTypeId,
                        UnitTypeName = x.U.UnitTypeName
                    }).FirstOrDefault();

                return productInfo;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }
    }
}
