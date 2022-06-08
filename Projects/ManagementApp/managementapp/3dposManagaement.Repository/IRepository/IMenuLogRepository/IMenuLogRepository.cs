using _3dposManagaement.Utility.MenuModel;
using CommonEntityModel.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.IRepository.IMenuLogRepository
{
    public interface IMenuLogRepository
    {
        Task<List<MenuLogModel>> GetMenuLogList(int loggedInUserId, string restaurantCode);
        Task<List<MenuLogModel>> GetMenuLogListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode);
        Task<List<MenuLogModel>> GetDateWiseMenuLogList(int loggedInUserId, string restaurantCode, string txtSelectedDateFrom, string txtSelectedDateTo);
    }
}
