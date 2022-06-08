using _3dposManagaement.Utility.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.IRepository.IProductPurchaseRepository
{
    public interface IProductPurchaseRepository
    {
        Task<PurchaseProductMargeModelsForMaster> GetProductPurchaseMasterList(int loggedInUserId, string restaurantCode);
        Task<PurchaseProductMargeModelsForMaster> GetProductPurchaseMasterListBySearchValue(string SearchText, string txtSelectedDateFrom, string txtSelectedDateTo, int loggedInUserId, string restaurantCode);
        Task<List<ProductCategoryList>> ProductCategoryList(int loggedInUserId, string restaurantCode);
        Task<List<ProductList>> ProductList(int loggedInUserId, string restaurantCode, int productCategoryId);
        Task<UniteTypeList> UniteTypeList(int loggedInUserId, string restaurantCode, int productId);
        Task<ProductInfo> ProductInfoByProductCategoryId(int loggedInUserId, string restaurantCode, int productCategoryId);
        int? GetProductPurchaseMasterMaxId(int loggedInUserId, string restaurantCode);
        ProductPurchaseCommonModel AddPurchaseProduct(ProductPurchaseCommonModel productPurchaseCommonModel, int loggedInUserId, string restaurantCode);
        Task<PurchaseProductMargeModels> GetProductPurchaseByPurchaseCode(int loggedInUserId, string restaurantCode, string purchaseCode);
        Task<List<PurchaseProductViewModel>> GetRestaurantWiseAllPurchaseList(int loggedInUserId, string restaurantCode);
        Task<List<PurchaseProductViewModel>> GetRestaurantWiseAllPurchaseListBySearchValue(string txtPurchaseCode, string txtProductCategoryName, string txtProductName, string txtUniteTypeName, string txtSelectedDateFrom, string txtSelectedDateTo, int loggedInUserId, string restaurantCode);
    }
}
