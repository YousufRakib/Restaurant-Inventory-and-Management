using _3dposManagaement.Repository.IRepository.IItemCategoryRepository;
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

namespace _3dposManagaement.Repository.Repository.ItemCategoryRepository
{
    public class ItemCategoryRepository : IItemCategoryRepository
    {
        private const string _areaName = "3DPOSManagement Module";
        private const string _controllerName = "ItemCategoryRepository";
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IConfiguration _configuration;
        private IHostingEnvironment hostingEnv;

        public ItemCategoryRepository(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository, IConfiguration configuration, IHostingEnvironment env)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._configuration = configuration;
            this.hostingEnv = env;
        }
        public async Task<ItemCategoryMargeViewModel> SaveItemCategory(ItemCategoryMargeViewModel itemCategoryData, IFormFile formFiles, int loggedInUserId, string restaurantCode)
        {
            string actionName = "SaveItemCategory";
            try
            {
                using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        string filePath = "";
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

                        ItemCategory itemCategory = new ItemCategory();
                        itemCategory.CategoryName = itemCategoryData.ItemCategoryViewModel.CategoryName;
                        itemCategory.CategoryDescription = itemCategoryData.ItemCategoryViewModel.CategoryDescription;
                        itemCategory.RestaurantCode = restaurantCode;
                        if (formFiles != null)
                        {
                            itemCategory.CategoryIconPath = filePath;
                        }
                        itemCategory.IsDeleted = false;
                        itemCategory.Status = true;
                        itemCategory.CreatedByName = userData.UserName;
                        itemCategory.CreatedBy = loggedInUserId;
                        itemCategory.CreatedDate = DateTime.UtcNow;

                        _DBContext.ItemCategory.Add(itemCategory);
                        _DBContext.SaveChanges();

                        foreach (var data in itemCategoryData.ItemCategoryWithMenuList)
                        {
                            CategoryWithMenu categoryWithMenu = new CategoryWithMenu();

                            categoryWithMenu.CategoryId = itemCategory.CategoryId;
                            categoryWithMenu.RestaurantCode = restaurantCode;
                            categoryWithMenu.MenuId = data.MenuId;
                            categoryWithMenu.CreatedByName = userData.UserName;
                            categoryWithMenu.CreatedBy = loggedInUserId;
                            categoryWithMenu.CreatedDate = DateTime.UtcNow;
                            _DBContext.CategoryWithMenu.Add(categoryWithMenu);
                            _DBContext.SaveChanges();

                        }
                        transaction.Commit();
                        return itemCategoryData;
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

        public async Task<List<ItemCategoryWithMenuViewModel>> GetMenuList(int loggedInUserId, string restaurantCode,int categoryId)
        {
            string actionName = "GetMenuList";
            try
            {
                string sqlQuery = @"select M.MenuId,M.MenuName,
                                    CASE WHEN (select MenuId from CategoryWithMenu  where MenuId=M.MenuId and CategoryId=@CategoryId) IS NOT NULL THEN 'True'
                                    ELSE 'False'
                                    END  as  IsCheckedMenu
                                    from Menu as M
                                    where M.RestaurantCode=@RestaurantCode And M.IsDeleted=0 group by M.MenuId,M.MenuName order by M.MenuId DESC";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<ItemCategoryWithMenuViewModel>(sqlQuery, new { CategoryId = categoryId, RestaurantCode = restaurantCode }).ToList();

                    return query;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<ItemCategoryViewModel>> ItemCategoryList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "ItemCategoryList";
            try
            {
                string sqlQuery = @"Select 
                                        ROW_NUMBER() Over (Order by IC.CategoryId desc) As Sl
                                        ,string_agg(M.MenuName, ',') as Menus
                                        ,IC.CategoryId
                                        ,IC.CategoryName
                                        ,IC.CategoryDescription
                                        ,IC.CategoryIconPath
                                        ,IC.RestaurantCode
                                        ,IC.Status
                                        ,IC.CreatedByName
                                        ,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,IC.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate
                                        ,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, IC.UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                        from ItemCategory as IC
                                        left join CategoryWithMenu as CM on IC.CategoryId=CM.CategoryId
                                        left join Menu as M on CM.MenuId=M.MenuId
                                        where IC.RestaurantCode=@RestaurantCode And IC.IsDeleted=0 
                                        group by IC.CategoryId,IC.CategoryName,IC.CategoryDescription,IC.CategoryIconPath,IC.RestaurantCode,IC.Status,IC.CreatedByName
                                        ,IC.CreatedDate,IC.UpdatedDate
                                        order by IC.CategoryId DESC";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<ItemCategoryViewModel>(sqlQuery, new { RestaurantCode = restaurantCode}).ToList();

                    return query;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<ItemCategoryViewModel>> ItemCategoryListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode)
        {
            string actionName = "ItemCategoryListBySearchValue";
            try
            {
                List<ItemCategoryViewModel> itemCategoryViewModel = new List<ItemCategoryViewModel>();

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    if (SearchText != null && SearchText != "")
                    {
                        string sqlQuery=@"Select 
                                        ROW_NUMBER() Over (Order by IC.CategoryId desc) As Sl
                                        ,string_agg(M.MenuName, ',') as Menus
                                        ,IC.CategoryId
                                        ,IC.CategoryName
                                        ,IC.CategoryDescription
                                        ,IC.CategoryIconPath
                                        ,IC.RestaurantCode
                                        ,IC.Status
                                        ,IC.CreatedByName
                                        ,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,IC.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate
                                        ,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, IC.UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                        from ItemCategory as IC
                                        left join CategoryWithMenu as CM on IC.CategoryId=CM.CategoryId
                                        left join Menu as M on CM.MenuId=M.MenuId
                                        where IC.RestaurantCode=@RestaurantCode And IC.CategoryName=@CategoryName And IC.IsDeleted=0 
                                        group by IC.CategoryId,IC.CategoryName,IC.CategoryDescription,IC.CategoryIconPath,IC.RestaurantCode,IC.Status,IC.CreatedByName
                                        ,IC.CreatedDate,IC.UpdatedDate
                                        order by IC.CategoryId DESC";

                        var query = connection.Query<ItemCategoryViewModel>(sqlQuery, new { RestaurantCode = restaurantCode, CategoryName = SearchText }).ToList();

                        return query;
                    }
                    else
                    {
                        string sqlQuery = @"Select 
                                        ROW_NUMBER() Over (Order by IC.CategoryId desc) As Sl
                                        ,string_agg(M.MenuName, ',') as Menus
                                        ,IC.CategoryId
                                        ,IC.CategoryName
                                        ,IC.CategoryDescription
                                        ,IC.CategoryIconPath
                                        ,IC.RestaurantCode
                                        ,IC.Status
                                        ,IC.CreatedByName
                                        ,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,IC.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate
                                        ,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, IC.UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                        from ItemCategory as IC
                                        left join CategoryWithMenu as CM on IC.CategoryId=CM.CategoryId
                                        left join Menu as M on CM.MenuId=M.MenuId
                                        where IC.RestaurantCode=@RestaurantCode And IC.IsDeleted=0 
                                        group by IC.CategoryId,IC.CategoryName,IC.CategoryDescription,IC.CategoryIconPath,IC.RestaurantCode,IC.Status,IC.CreatedByName
                                        ,IC.CreatedDate,IC.UpdatedDate
                                        order by IC.CategoryId DESC";

                        var query = connection.Query<ItemCategoryViewModel>(sqlQuery, new { RestaurantCode = restaurantCode}).ToList();

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

        public async Task<ItemCategoryMargeViewModel> GetItemCategoryById(int loggedInUserId, string restaurantCode, int categoryId)
        {
            string actionName = "GetItemCategoryById";
            try
            {
                ItemCategoryMargeViewModel itemCategoryMargeViewModel = new ItemCategoryMargeViewModel();
                ItemCategory itemCategory = await _DBContext.ItemCategory.Where(x => x.CategoryId == categoryId && x.IsDeleted == false && x.RestaurantCode==restaurantCode).FirstOrDefaultAsync();

                itemCategoryMargeViewModel.CategoryId = itemCategory.CategoryId;
                itemCategoryMargeViewModel.CategoryName = itemCategory.CategoryName;
                itemCategoryMargeViewModel.CategoryDescription = itemCategory.CategoryDescription;
                itemCategoryMargeViewModel.CategoryIconPath = itemCategory.CategoryIconPath;
                itemCategoryMargeViewModel.Status = itemCategory.Status;
                itemCategoryMargeViewModel.IsDeleted = itemCategory.IsDeleted;
                itemCategoryMargeViewModel.ItemCategoryList = new List<ItemCategoryViewModel>();
                itemCategoryMargeViewModel.ItemCategoryWithMenuList = new List<ItemCategoryWithMenuViewModel>();

                return itemCategoryMargeViewModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<string> DeleteItemCategoryById(int loggedInUserId, string restaurantCode, int itemCategoryId, string code)
        {
            string actionName = "DeleteItemCategoryById";
            try
            {
                using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        var userData = await _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefaultAsync();
                        ItemCategory itemCategory = await _DBContext.ItemCategory.Where(x => x.CategoryId == itemCategoryId && x.IsDeleted == false && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();
                        var verificationData = await _DBContext.VarificationCode.Where(x => x.Username == userData.UserName && x.IsDeleted == false && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                        if (verificationData.Code == code)
                        {
                            if (itemCategory != null)
                            {
                                MenuLog menuLog = new MenuLog();
                                menuLog.ChangeType = "Category Delete";
                                menuLog.ChangeItemName = itemCategory.CategoryName;
                                menuLog.OldValue = "Active";
                                menuLog.UpdatedValue = "Deleted";
                                menuLog.ChangedObject = "ItemCategory";
                                menuLog.RestaurantCode = restaurantCode;
                                menuLog.CreatedByName = userData.UserName;
                                menuLog.CreatedBy = loggedInUserId;
                                menuLog.CreatedDate = DateTime.UtcNow;
                                _DBContext.MenuLog.Add(menuLog);
                                _DBContext.SaveChanges();

                                itemCategory.IsDeleted = true;
                                itemCategory.Status = false;
                                itemCategory.UpdatedBy = loggedInUserId;
                                itemCategory.UpdatedByName = userData.UserName;
                                itemCategory.UpdatedDate = DateTime.UtcNow;
                                _DBContext.SaveChanges();

                                var categoryWithMenu = await _DBContext.CategoryWithMenu.Where(x => x.CategoryId == itemCategory.CategoryId && x.RestaurantCode == restaurantCode).ToListAsync();
                                foreach (var data in categoryWithMenu)
                                {
                                    var menu = _DBContext.Menu.Where(x => x.MenuId == data.MenuId).FirstOrDefault();

                                    menu.UpdatedBy = loggedInUserId;
                                    menu.UpdatedByName = userData.UserName;
                                    menu.UpdatedDate = DateTime.UtcNow;
                                    _DBContext.SaveChanges();
                                }
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

        public async Task<string> ChangeCategoryStatusById(int loggedInUserId, string restaurantCode, int categoryId)
        {
            string actionName = "ChangeCategoryStatusById";
            try
            {
                using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();
                        ItemCategory itemCategory = await _DBContext.ItemCategory.Where(x => x.CategoryId == categoryId && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();
                        
                        if (itemCategory != null)
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "Status Change";
                            menuLog.ChangeItemName = itemCategory.CategoryName;
                            menuLog.OldValue = "Active";
                            menuLog.UpdatedValue = "Inactive";
                            menuLog.ChangedObject = "ItemCategory";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();

                            if (itemCategory.Status == true)
                            {
                                itemCategory.Status = false;
                            }
                            else
                            {
                                itemCategory.Status = true;
                            }
                            itemCategory.UpdatedBy = loggedInUserId;
                            itemCategory.UpdatedByName = userData.UserName;
                            itemCategory.UpdatedDate = DateTime.UtcNow;
                            _DBContext.SaveChanges();

                            var categoryWithMenu = await _DBContext.CategoryWithMenu.Where(x => x.CategoryId == itemCategory.CategoryId && x.RestaurantCode == restaurantCode).ToListAsync();
                            foreach (var data in categoryWithMenu)
                            {
                                var menu = _DBContext.Menu.Where(x => x.MenuId == data.MenuId).FirstOrDefault();

                                menu.UpdatedBy = loggedInUserId;
                                menu.UpdatedByName = userData.UserName;
                                menu.UpdatedDate = DateTime.UtcNow;
                                _DBContext.SaveChanges();
                            }
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

        public async Task<ItemCategoryMargeViewModel> UpdateItemCategory(ItemCategoryMargeViewModel itemCategoryData, IFormFile formFiles, int loggedInUserId, string restaurantCode)
        {
            string actionName = "UpdateItemCategory";
            try
            {
                using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        string filePath = "";
                        var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();
                        ItemCategory ItemCategory = await _DBContext.ItemCategory.Where(x => x.CategoryId == itemCategoryData.ItemCategoryViewModel.CategoryId && x.IsDeleted == false && x.RestaurantCode==restaurantCode).FirstOrDefaultAsync();
                        

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

                        if (ItemCategory.CategoryName != itemCategoryData.ItemCategoryViewModel.CategoryName)
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "Category Change";
                            menuLog.ChangeItemName = ItemCategory.CategoryName;
                            menuLog.OldValue = ItemCategory.CategoryName;
                            menuLog.UpdatedValue = itemCategoryData.ItemCategoryViewModel.CategoryName;
                            menuLog.ChangedObject = "ItemCategory";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();
                        }
                        ItemCategory.CategoryName = itemCategoryData.ItemCategoryViewModel.CategoryName;

                        if (ItemCategory.CategoryDescription != itemCategoryData.ItemCategoryViewModel.CategoryDescription)
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "Description Change";
                            menuLog.ChangeItemName = ItemCategory.CategoryName;
                            menuLog.OldValue = ItemCategory.CategoryDescription;
                            menuLog.UpdatedValue = itemCategoryData.ItemCategoryViewModel.CategoryDescription;
                            menuLog.ChangedObject = "ItemCategory";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();
                        }
                        ItemCategory.CategoryDescription = itemCategoryData.ItemCategoryViewModel.CategoryDescription;

                        ItemCategory.RestaurantCode = restaurantCode;
                        if (formFiles != null)
                        {
                            if (ItemCategory.CategoryIconPath != filePath)
                            {
                                MenuLog menuLog = new MenuLog();
                                menuLog.ChangeType = "Image Change";
                                menuLog.ChangeItemName = ItemCategory.CategoryName;
                                menuLog.OldValue = ItemCategory.CategoryIconPath;
                                menuLog.UpdatedValue = filePath;
                                menuLog.ChangedObject = "ItemCategory";
                                menuLog.RestaurantCode = restaurantCode;
                                menuLog.CreatedByName = userData.UserName;
                                menuLog.CreatedBy = loggedInUserId;
                                menuLog.CreatedDate = DateTime.UtcNow;
                                _DBContext.MenuLog.Add(menuLog);
                                _DBContext.SaveChanges();
                            }
                            ItemCategory.CategoryIconPath = filePath;
                        }
                        ItemCategory.UpdatedByName = userData.UserName;
                        ItemCategory.UpdatedBy = loggedInUserId;
                        ItemCategory.UpdatedDate = DateTime.UtcNow;
                        _DBContext.SaveChanges();

                        string previousMenus = "";
                        string updatedMenus = "";

                        var removeData = _DBContext.CategoryWithMenu.Where(x => x.CategoryId == ItemCategory.CategoryId && x.RestaurantCode==restaurantCode).ToList();
                        
                        foreach (var data in removeData)
                        {
                            var menus = _DBContext.Menu.Where(x => x.MenuId == data.MenuId).Select(x => x.MenuName).FirstOrDefault();
                            previousMenus = previousMenus + menus + ",";

                            _DBContext.CategoryWithMenu.Remove(data);
                            _DBContext.SaveChanges();
                        }
                        
                        foreach (var data in itemCategoryData.ItemCategoryWithMenuList)
                        {
                            var menus = _DBContext.Menu.Where(x => x.MenuId == data.MenuId).FirstOrDefault();
                            updatedMenus = updatedMenus + menus.MenuName + ",";

                            menus.UpdatedBy = loggedInUserId;
                            menus.UpdatedByName = userData.UserName;
                            menus.UpdatedDate = DateTime.UtcNow;
                            _DBContext.SaveChanges();

                            CategoryWithMenu categoryWithMenu = new CategoryWithMenu();
                            categoryWithMenu.CategoryId = ItemCategory.CategoryId;
                            categoryWithMenu.RestaurantCode = restaurantCode;
                            categoryWithMenu.MenuId = data.MenuId;
                            categoryWithMenu.CreatedByName = userData.UserName;
                            categoryWithMenu.CreatedBy = loggedInUserId;
                            categoryWithMenu.CreatedDate = DateTime.UtcNow;
                            _DBContext.CategoryWithMenu.Add(categoryWithMenu);
                            _DBContext.SaveChanges();
                        }
                        if (previousMenus != updatedMenus)
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "Category in Menu";
                            menuLog.ChangeItemName = ItemCategory.CategoryName;
                            menuLog.OldValue = previousMenus.Substring(0, previousMenus.Length - 1);
                            menuLog.UpdatedValue = updatedMenus.Substring(0, updatedMenus.Length - 1);
                            menuLog.ChangedObject = "ItemCategory";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();
                        }

                        transaction.Commit();
                        return itemCategoryData;
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
