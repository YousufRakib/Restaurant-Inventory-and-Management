using _3dposManagaement.Utility.CommonModel;
using CommonEntityModel.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.IRepository.IProductCategoryRepository
{
    public interface IProductCategoryRepository
    {
        ProductCategory SaveProductCategory(ProductCategory productCategory, int loggedInUserId, string restaurantCode);
        Task<List<ProductCategoryViewModel>> GetProductCategory(int loggedInUserId, string restaurantCode);
        Task<List<ProductCategoryViewModel>> GetProductCategoryBySearchValue(string SearchText, int loggedInUserId, string restaurantCode);
        Task<ProductCategory> GetProductCategoryByID(int id, int loggedInUserId, string restaurantCode);
        ProductCategory EditProductCategory(ProductCategory productCategory, int loggedInUserId, string restaurantCode);
        Task<string> RemoveProductCategory(int id, string code, int loggedInUserId, string restaurantCode);
        Task<string> CheckExistProductCategory(ProductCategory productCategory, int loggedInUserId, string restaurantCode);
    }
}
