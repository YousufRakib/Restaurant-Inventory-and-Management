using _3dposManagaement.Utility.CommonModel;
using CommonEntityModel.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.IRepository.IProductRepository
{
    public interface IProductRepository
    {
        Product SaveProduct(Product product, int loggedInUserId, string restaurantCode);
        Task<List<ProductModel>> GetProductList(int loggedInUserId, string restaurantCode);
        Task<List<ProductModel>> GetProductListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode);
        Task<ProductModel> GetProductByID(int id, int loggedInUserId, string restaurantCode);
        Product EditProduct(Product product, int loggedInUserId, string restaurantCode);
        Task<string> RemoveProduct(int id, string code, int loggedInUserId, string restaurantCode);
        Task<string> CheckExistProduct(Product product, int loggedInUserId, string restaurantCode);
        Task<List<UnitType>> UnitTypeList(int loggedInUserId, string restaurantCode);
        Task<List<ProductCategory>> ProductCategoryList(int loggedInUserId, string restaurantCode);
    }
}
