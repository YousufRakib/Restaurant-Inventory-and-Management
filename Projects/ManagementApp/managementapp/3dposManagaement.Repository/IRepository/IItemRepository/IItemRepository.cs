using _3dposManagaement.Utility.MenuModel;
using CommonEntityModel.EntityModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.IRepository.IItemRepository
{
    public interface IItemRepository
    {
        Task<List<ItemCategoryWithMenuViewModel>> GetMenuList(int loggedInUserId, string restaurantCode, int itemId);
        Task<List<ItemWithCategoryModel>> GetCategoryList(int loggedInUserId, string restaurantCode, int itemId);
        Task<ItemMargeViewModel> GetItemById(int loggedInUserId, string restaurantCode, int itemId);
        Task<ItemMargeViewModel> SaveItem(ItemMargeViewModel itemMargeViewModelData, IFormFile formFiles, int loggedInUserId, string restaurantCode);
        Task<ItemMargeViewModel> UpdateItem(ItemMargeViewModel itemMargeViewModelData, IFormFile formFiles, int loggedInUserId, string restaurantCode);
        Task<string> DeleteItemById(int loggedInUserId, string restaurantCode, int itemId, string code);
        Task<List<VariantMaster>> VariantMasterList(int loggedInUserId, string restaurantCode);
        Task<List<ItemVariant>> VariantList(int loggedInUserId, string restaurantCode, int variantMasterId);
        Task<List<ItemVariant>> VariantListWithPrice(int loggedInUserId, string restaurantCode, int variantMasterId, int itemId);
        Task<ItemMargeViewModel> GetItemListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode);
        Task<ItemMargeViewModel> GetItemList(int loggedInUserId, string restaurantCode);
        Task<string> ChangeItemStatusById(int loggedInUserId, string restaurantCode, int itemId);
    }
}
