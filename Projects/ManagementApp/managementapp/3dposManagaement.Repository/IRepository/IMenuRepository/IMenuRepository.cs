using _3dposManagaement.Utility.MenuModel;
using CommonEntityModel.EntityModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.IRepository.IMenuRepository
{
    public interface IMenuRepository
    {
        Task<Menu> SaveMenu(MenuViewModel menuData, IFormFile formFiles, int loggedInUserId, string restaurantCode);
        Task<List<MenuViewModel>> GetMenuList(int loggedInUserId, string restaurantCode);
        Task<List<MenuViewModel>> GetMenuListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode);
        Task<MenuMargeViewModel> GetMenuById(int loggedInUserId, string restaurantCode, int menuId);
        Task<string> DeleteMenuById(int loggedInUserId, string restaurantCode, int menuId, string code);
        Task<Menu> UpdateMenu(MenuViewModel menuData, IFormFile formFiles, int loggedInUserId, string restaurantCode);
        Task<string> ChangeMenuStatusById(int loggedInUserId, string restaurantCode, int menuId);
    }
}
