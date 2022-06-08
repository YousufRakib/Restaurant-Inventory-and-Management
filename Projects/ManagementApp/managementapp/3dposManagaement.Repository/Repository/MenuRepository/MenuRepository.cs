using _3dposManagaement.Repository.IRepository.IMenuRepository;
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
using Dapper;
using _3dposManagaement.Utility.MenuModel;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;

namespace _3dposManagaement.Repository.Repository.MenuRepository
{
    public class MenuRepository : IMenuRepository
    {
        private const string _areaName = "3DPOSManagement Module";
        private const string _controllerName = "MenuRepository";
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IConfiguration _configuration;
        private IHostingEnvironment hostingEnv;

        public MenuRepository(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository, IConfiguration configuration, IHostingEnvironment env)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._configuration = configuration;
            this.hostingEnv = env;
        }

        public async Task<Menu> SaveMenu(MenuViewModel menuData,IFormFile formFiles, int loggedInUserId, string restaurantCode)
        {
            string actionName = "SaveMenu";
            try
            {
                string filePath="";
                var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();

                if (formFiles != null)
                {
                    var FileDic = "Files";
                    string FilePath = Path.Combine(hostingEnv.WebRootPath, FileDic);

                    if (!Directory.Exists(FilePath))
                        Directory.CreateDirectory(FilePath);

                    var fileName = formFiles.FileName;
                    filePath = Path.Combine(FilePath, fileName);
                    using (FileStream fs = System.IO.File.Create(filePath))
                    {
                        formFiles.CopyTo(fs);
                    }
                }

                Menu menu = new Menu();

                menu.MenuName = menuData.MenuName;
                menu.MenuDescription = menuData.MenuDescription;
                menu.RestaurantCode = restaurantCode;
                menu.MenuImagePath = filePath;
                menu.IsDeleted = false;
                menu.Status = true;
                menu.CreatedByName = userData.UserName;
                menu.CreatedBy = loggedInUserId;
                menu.CreatedDate = DateTime.UtcNow;


                _DBContext.Menu.Add(menu);
                _DBContext.SaveChanges();

                return menu;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<MenuViewModel>> GetMenuList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetMenuList";
            try
            {
                string sqlQuery = @"select ROW_NUMBER() Over (Order by MenuId desc) As Sl,MenuId,MenuImagePath, RestaurantCode,MenuName,MenuDescription,Status,CreatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from Menu 
                                    where RestaurantCode='" + restaurantCode + "' And IsDeleted=0 order by MenuId DESC";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<MenuViewModel>(sqlQuery).ToList();

                    return query;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<MenuViewModel>> GetMenuListBySearchValue(string SearchText,int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetMenuListBySearchValue";
            try
            {
                List<MenuViewModel> menu = new List<MenuViewModel>();

                string sqlQuery = "";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    if (SearchText != null && SearchText != "")
                    {
                        sqlQuery = @"select ROW_NUMBER() Over (Order by MenuId desc) As Sl,MenuId,MenuImagePath, RestaurantCode,MenuName,MenuDescription,Status,CreatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from Menu 
                                    where RestaurantCode=@RestaurantCode And MenuName=@MenuName And IsDeleted=0 order by MenuId DESC";

                        var query = connection.Query<MenuViewModel>(sqlQuery, new { RestaurantCode = restaurantCode, MenuName = SearchText}).ToList();

                        return query;
                    }
                    else
                    {
                        sqlQuery = @"select ROW_NUMBER() Over (Order by MenuId desc) As Sl,MenuId,MenuImagePath, RestaurantCode,MenuName,MenuDescription,Status,CreatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from Menu 
                                    where RestaurantCode=@RestaurantCode And IsDeleted=0 order by MenuId DESC";

                        var query = connection.Query<MenuViewModel>(sqlQuery, new { RestaurantCode = restaurantCode}).ToList();

                        return query;
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<MenuMargeViewModel> GetMenuById(int loggedInUserId, string restaurantCode, int menuId)
        {
            string actionName = "GetMenuById";
            try
            {
                MenuMargeViewModel menuViewModel = new MenuMargeViewModel();
                Menu menu = await _DBContext.Menu.Where(x => x.MenuId == menuId && x.IsDeleted==false && x.RestaurantCode==restaurantCode).FirstOrDefaultAsync();

                menuViewModel.MenuId = menu.MenuId;
                menuViewModel.MenuName = menu.MenuName;
                menuViewModel.MenuDescription = menu.MenuDescription;
                menuViewModel.MenuImagePath = menu.MenuImagePath;
                menuViewModel.Status = menu.Status;
                menuViewModel.IsDeleted = menu.IsDeleted;
                menuViewModel.menuList = new List<MenuViewModel>();

                return menuViewModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<string> DeleteMenuById(int loggedInUserId, string restaurantCode, int menuId,string code)
        {
            string actionName = "DeleteMenuById";
            try
            {
                using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();

                        Menu menu = await _DBContext.Menu.Where(x => x.MenuId == menuId && x.IsDeleted == false && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();
                        var verificationData = await _DBContext.VarificationCode.Where(x => x.Username == userData.UserName && x.IsDeleted == false && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                        if (verificationData.Code == code)
                        {
                            if (menu != null)
                            {
                                MenuLog menuLog = new MenuLog();
                                menuLog.ChangeType = "Menu Delete";
                                menuLog.ChangeItemName =menu.MenuName;
                                menuLog.OldValue = "Active";
                                menuLog.UpdatedValue = "Deleted";
                                menuLog.ChangedObject = "Menu";
                                menuLog.RestaurantCode = restaurantCode;
                                menuLog.CreatedByName = userData.UserName;
                                menuLog.CreatedBy = loggedInUserId;
                                menuLog.CreatedDate = DateTime.UtcNow;
                                _DBContext.MenuLog.Add(menuLog);
                                _DBContext.SaveChanges();

                                menu.IsDeleted = true;
                                menu.Status = false;
                                menu.UpdatedBy = loggedInUserId;
                                menu.UpdatedByName = userData.UserName;
                                menu.UpdatedDate = DateTime.UtcNow;
                                _DBContext.SaveChanges();

                                transaction.Commit();
                                return "True";
                            }
                        }
                        return "False";
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
                return "Error";
            }
        }

        public async Task<string> ChangeMenuStatusById(int loggedInUserId, string restaurantCode, int menuId)
        {
            string actionName = "ChangeMenuStatusById";
            try
            {
                using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();

                        Menu menu = await _DBContext.Menu.Where(x => x.MenuId == menuId && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();
                        
                        if (menu != null)
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "Status Change";
                            menuLog.ChangeItemName = menu.MenuName;
                            menuLog.OldValue = "Active";
                            menuLog.UpdatedValue = "Inactive";
                            menuLog.ChangedObject = "Menu";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();

                            if (menu.Status == true)
                            {
                                menu.Status = false;
                            }
                            else
                            {
                                menu.Status = true;
                            }
                            menu.UpdatedBy = loggedInUserId;
                            menu.UpdatedByName = userData.UserName;
                            menu.UpdatedDate = DateTime.UtcNow;
                            _DBContext.SaveChanges();

                            transaction.Commit();
                            return "True";
                        }
                        return null;
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
                return "Error";
            }
        }

        public async Task<Menu> UpdateMenu(MenuViewModel menuData, IFormFile formFiles, int loggedInUserId, string restaurantCode)
        {
            string actionName = "UpdateMenu";
            try
            {
                using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        string filePath = "";
                        if (formFiles != null)
                        {
                            var FileDic = "Files";
                            string FilePath = Path.Combine(hostingEnv.WebRootPath, FileDic);

                            if (!Directory.Exists(FilePath))
                                Directory.CreateDirectory(FilePath);

                            var fileName = formFiles.FileName;
                            filePath = Path.Combine(FilePath, fileName);
                            using (FileStream fs = System.IO.File.Create(filePath))
                            {
                                formFiles.CopyTo(fs);
                            }
                        }

                        var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();
                        Menu menu = await _DBContext.Menu.Where(x => x.MenuId == menuData.MenuId && x.IsDeleted == false && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                        if (menu.MenuName != menuData.MenuName)
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "Menu Change";
                            menuLog.ChangeItemName = menu.MenuName;
                            menuLog.OldValue = menu.MenuName;
                            menuLog.UpdatedValue = menuData.MenuName;
                            menuLog.ChangedObject = "Menu";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();
                        }
                        menu.MenuName = menuData.MenuName;

                        if (menu.MenuDescription != menuData.MenuDescription)
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "Description Change";
                            menuLog.ChangeItemName = menu.MenuName;
                            menuLog.OldValue = menu.MenuDescription;
                            menuLog.UpdatedValue = menuData.MenuDescription;
                            menuLog.ChangedObject = "Menu";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();
                        }
                        menu.MenuDescription = menuData.MenuDescription;

                        menu.RestaurantCode = restaurantCode;
                        if (formFiles != null)
                        {
                            if (menu.MenuImagePath != filePath)
                            {
                                MenuLog menuLog = new MenuLog();
                                menuLog.ChangeType = "Image Change";
                                menuLog.ChangeItemName = menu.MenuName;
                                menuLog.OldValue = menu.MenuImagePath;
                                menuLog.UpdatedValue = filePath;
                                menuLog.ChangedObject = "Menu";
                                menuLog.RestaurantCode = restaurantCode;
                                menuLog.CreatedByName = userData.UserName;
                                menuLog.CreatedBy = loggedInUserId;
                                menuLog.CreatedDate = DateTime.UtcNow;
                                _DBContext.MenuLog.Add(menuLog);
                                _DBContext.SaveChanges();
                            }
                            menu.MenuImagePath = filePath;
                        }
                        menu.UpdatedByName = userData.UserName;
                        menu.UpdatedBy = loggedInUserId;
                        menu.UpdatedDate = DateTime.UtcNow;

                        _DBContext.SaveChanges();
                        transaction.Commit();
                        return menu;
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
    }
}
