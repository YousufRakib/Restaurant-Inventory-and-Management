using _3dposManagaement.Utility.CommonModel;
using CommonEntityModel.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.IRepository.IInventoryRepository
{
    public interface IInventoryRepository
    {
        //InventoryModel SaveInventory(InventoryModel inventory, int loggedInUserId, string restaurantCode);
        //InventoryModel EditInventory(InventoryModel inventory, int loggedInUserId, string restaurantCode);
        Task<InventoryModel> GetInventoryByID(int id, int loggedInUserId, string restaurantCode);
        //Task<List<InventoryViewModel>> GetInventoryListBySearchValue(string txtInventoryId, string txtProductName,string txtSelectedDateFrom, string txtSelectedDateTo, int loggedInUserId, string restaurantCode);
        Task<List<InventoryViewModel>> GetInventoryListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode);
        Task<List<InventoryViewModel>> GetInventoryList(int loggedInUserId, string restaurantCode);
        //Task<string> RemoveInventory(int id, int loggedInUserId, string restaurantCode);
    }
}
