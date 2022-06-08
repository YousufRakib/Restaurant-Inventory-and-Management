using _3dposManagaement.Utility.MenuModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.IRepository.IItemCategoryRepository
{
    public interface IItemCategoryRepository
    {
        Task<ItemCategoryMargeViewModel> SaveItemCategory(ItemCategoryMargeViewModel itemCategoryData, IFormFile formFiles, int loggedInUserId, string restaurantCode);
        Task<List<ItemCategoryViewModel>> ItemCategoryList(int loggedInUserId, string restaurantCode);
        Task<List<ItemCategoryViewModel>> ItemCategoryListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode);
        Task<ItemCategoryMargeViewModel> GetItemCategoryById(int loggedInUserId, string restaurantCode, int categoryId);
        Task<string> DeleteItemCategoryById(int loggedInUserId, string restaurantCode, int itemCategoryId, string code);
        Task<ItemCategoryMargeViewModel> UpdateItemCategory(ItemCategoryMargeViewModel itemCategoryData, IFormFile formFiles, int loggedInUserId, string restaurantCode);
        Task<List<ItemCategoryWithMenuViewModel>> GetMenuList(int loggedInUserId, string restaurantCode,int categoryId);
        Task<string> ChangeCategoryStatusById(int loggedInUserId, string restaurantCode, int categoryId);
    }
}
