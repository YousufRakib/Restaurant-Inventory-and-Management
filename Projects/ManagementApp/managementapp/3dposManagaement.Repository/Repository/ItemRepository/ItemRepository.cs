using _3dposManagaement.Repository.IRepository.IItemRepository;
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
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace _3dposManagaement.Repository.Repository.ItemRepository
{
    public class ItemRepository : IItemRepository
    {
        private const string _areaName = "3DPOSManagement Module";
        private const string _controllerName = "ItemRepository";
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;
        private IHostingEnvironment _hostingEnv;

        public ItemRepository(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository,IHostingEnvironment hostingEnv)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._hostingEnv = hostingEnv;
        }

        public async Task<List<ItemCategoryWithMenuViewModel>> GetMenuList(int loggedInUserId, string restaurantCode, int itemId)
        {
            string actionName = "GetMenuList";
            try
            {
                string sqlQuery = @"select M.MenuId,M.MenuName,
                                    CASE WHEN (select MenuId from ItemWithMenu  where MenuId=M.MenuId and ItemId=@ItemId) IS NOT NULL THEN 'True'
                                    ELSE 'False'
                                    END  as  IsCheckedMenu
                                    from Menu as M
                                    where M.RestaurantCode=@RestaurantCode And M.IsDeleted=0 
                                    group by M.MenuId,M.MenuName order by M.MenuId DESC";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<ItemCategoryWithMenuViewModel>(sqlQuery, new { ItemId = itemId, RestaurantCode = restaurantCode }).ToList();

                    return query;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<ItemWithCategoryModel>> GetCategoryList(int loggedInUserId, string restaurantCode, int itemId)
        {
            string actionName = "GetCategoryList";
            try
            {
                string sqlQuery = @"select IC.CategoryId,IC.CategoryName,
                                    CASE WHEN (select CategoryId from ItemWithCategory  where CategoryId=IC.CategoryId and ItemId=@ItemId) IS NOT NULL THEN 'True'
                                    ELSE 'False'
                                    END  as  IsCheckedCategory
                                    from ItemCategory as IC
                                    where IC.RestaurantCode=@RestaurantCode And IC.IsDeleted=0 
                                    group by IC.CategoryId,IC.CategoryName order by IC.CategoryId DESC";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<ItemWithCategoryModel>(sqlQuery, new { ItemId = itemId, RestaurantCode = restaurantCode }).ToList();

                    return query;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<ItemMargeViewModel> GetItemById(int loggedInUserId, string restaurantCode, int itemId)
        {
            string actionName = "GetItemById";
            try
            {
                ItemMargeViewModel itemMargeViewModel = new ItemMargeViewModel();
                Item item = await _DBContext.Item.Where(x => x.ItemId == itemId && x.IsDeleted == false && x.RestaurantCode==restaurantCode).FirstOrDefaultAsync();

                itemMargeViewModel.ItemId = item.ItemId;
                itemMargeViewModel.ItemName = item.ItemName;
                itemMargeViewModel.ItemDescription = item.ItemDescription;
                itemMargeViewModel.ItemImagePath = item.ItemImagePath;
                itemMargeViewModel.Status = item.Status;
                itemMargeViewModel.IsDeleted = item.IsDeleted;
                itemMargeViewModel.ItemListModels = new List<ItemListModel>();
                itemMargeViewModel.ItemListModel = new ItemListModel();
                itemMargeViewModel.VariantAndPriceModel = new List<VariantAndPriceModel>();
                itemMargeViewModel.ItemWithMenuModel = new List<ItemWithMenuModel>();
                itemMargeViewModel.ItemWithCategoryModel = new List<ItemWithCategoryModel>();
                itemMargeViewModel.VariantMasterId = item.VariantMasterId;
                itemMargeViewModel.VariantMaster = _DBContext.VariantMaster.Where(o => o.RestaurantCode == restaurantCode).ToList();

                return itemMargeViewModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<VariantMaster>> VariantMasterList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "VariantMasterList";
            try
            {
                List<VariantMaster> variantMasterData = await _DBContext.VariantMaster.Where(x => x.RestaurantCode == restaurantCode).ToListAsync();

                return variantMasterData;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<ItemVariant>> VariantList(int loggedInUserId, string restaurantCode, int variantMasterId )
        {
            string actionName = "VariantList";
            
            try
            {
                string sqlQuery = @"Select V.VariantId,V.VariantName,0 as VariantWiseItemprice from Variant as V
                                    left join ItemVariant as IV on V.VariantId=IV.VariantId
                                    where V.VariantMasterId=@VariantMasterId And V.RestaurantCode=@RestaurantCode";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<ItemVariant>(sqlQuery, new { VariantMasterId= variantMasterId, RestaurantCode = restaurantCode }).ToList();

                    return query;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<ItemVariant>> VariantListWithPrice(int loggedInUserId, string restaurantCode, int variantMasterId, int itemId)
        {
            string actionName = "VariantListWithPrice";

            try
            {
                string sqlQuery = @"Select V.VariantId,V.VariantName,IV.VariantWiseItemprice from Variant as V
                                    left join ItemVariant as IV on V.VariantId=IV.VariantId
                                    where V.VariantMasterId=@VariantMasterId And V.RestaurantCode=@RestaurantCode And IV.ItemId=@ItemId";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<ItemVariant>(sqlQuery, new { VariantMasterId = variantMasterId, RestaurantCode = restaurantCode, ItemId=itemId }).ToList();

                    return query;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<ItemMargeViewModel> SaveItem(ItemMargeViewModel itemMargeViewModelData, IFormFile formFiles, int loggedInUserId, string restaurantCode)
        {
            string actionName = "SaveItem";
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
                            string FilePath = Path.Combine(_hostingEnv.WebRootPath, FileDic);

                            if (!Directory.Exists(FilePath))
                                Directory.CreateDirectory(FilePath);

                            var fileName = formFiles.FileName;
                            filePath = Path.Combine(FilePath, fileName);
                            using (FileStream fs = System.IO.File.Create(filePath))
                            {
                                formFiles.CopyTo(fs);
                            }
                        }

                        Item item = new Item();
                        item.VariantMasterId =Convert.ToInt32(itemMargeViewModelData.ItemListModel.VariantMasterId);
                        item.ItemName = itemMargeViewModelData.ItemListModel.ItemName;
                        item.ItemDescription = itemMargeViewModelData.ItemListModel.ItemDescription;
                        item.RestaurantCode = restaurantCode;
                        if (formFiles != null)
                        {
                            item.ItemImagePath = filePath;
                        }
                        item.IsDeleted = false;
                        item.Status = true;
                        item.CreatedByName = userData.UserName;
                        item.CreatedBy = loggedInUserId;
                        item.CreatedDate = DateTime.UtcNow;

                        _DBContext.Item.Add(item);
                        _DBContext.SaveChanges();

                        foreach (var data in itemMargeViewModelData.ItemWithMenuModel)
                        {
                            ItemWithMenu itemWithMenu = new ItemWithMenu();
                            itemWithMenu.ItemId = item.ItemId;
                            itemWithMenu.RestaurantCode = restaurantCode;
                            itemWithMenu.MenuId = data.MenuId;
                            itemWithMenu.CreatedByName = userData.UserName;
                            itemWithMenu.CreatedBy = loggedInUserId;
                            itemWithMenu.CreatedDate = DateTime.UtcNow;
                            _DBContext.ItemWithMenu.Add(itemWithMenu);
                            _DBContext.SaveChanges();
                        }

                        foreach (var data in itemMargeViewModelData.ItemWithCategoryModel)
                        {
                            ItemWithCategory itemWithCategory = new ItemWithCategory();
                            itemWithCategory.ItemId = item.ItemId;
                            itemWithCategory.RestaurantCode = restaurantCode;
                            itemWithCategory.CategoryId = data.CategoryId;
                            itemWithCategory.CreatedByName = userData.UserName;
                            itemWithCategory.CreatedBy = loggedInUserId;
                            itemWithCategory.CreatedDate = DateTime.UtcNow;
                            _DBContext.ItemWithCategory.Add(itemWithCategory);
                            _DBContext.SaveChanges();
                        }

                        foreach (var data in itemMargeViewModelData.VariantPriceModel) 
                        {
                            ItemVariant itemVariant = new ItemVariant();
                            itemVariant.ItemId = item.ItemId;
                            itemVariant.VariantId = data.VariantId;
                            itemVariant.VariantWiseItemprice = data.VariantWiseItemprice;
                            itemVariant.VariantName = data.VariantName;
                            itemVariant.VariantMasterId = item.VariantMasterId;
                            itemVariant.RestaurantCode = restaurantCode;
                            itemVariant.CreatedByName = userData.UserName;
                            itemVariant.CreatedBy = loggedInUserId;
                            itemVariant.CreatedDate = DateTime.UtcNow;
                            _DBContext.ItemVariant.Add(itemVariant);
                            _DBContext.SaveChanges();
                        }

                        transaction.Commit();
                        return itemMargeViewModelData;
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

        public async Task<ItemMargeViewModel> UpdateItem(ItemMargeViewModel itemMargeViewModelData, IFormFile formFiles, int loggedInUserId, string restaurantCode)
        {
            string actionName = "UpdateItem";
            try
            {
                using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        string filePath = "";
                        var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();
                        Item item = await _DBContext.Item.Where(x => x.ItemId == itemMargeViewModelData.ItemListModel.ItemId && x.IsDeleted == false && x.RestaurantCode==restaurantCode).FirstOrDefaultAsync();

                        if (formFiles != null)
                        {
                            var FileDic = "Files";
                            string FilePath = Path.Combine(_hostingEnv.WebRootPath, FileDic);

                            if (!Directory.Exists(FilePath))
                                Directory.CreateDirectory(FilePath);

                            var fileName = formFiles.FileName;
                            filePath = Path.Combine(FilePath, fileName);
                            using (FileStream fs = System.IO.File.Create(filePath))
                            {
                                formFiles.CopyTo(fs);
                            }
                        }

                        var previousVariantMasterName = await _DBContext.VariantMaster.Where(x => x.VariantMasterId == item.VariantMasterId).Select(x => x.VariantMasterName).FirstOrDefaultAsync();
                        var updatedVariantMasterName =await _DBContext.VariantMaster.Where(x => x.VariantMasterId == Convert.ToInt32(itemMargeViewModelData.ItemListModel.VariantMasterId)).Select(x => x.VariantMasterName).FirstOrDefaultAsync();
                        if (item.VariantMasterId != Convert.ToInt32(itemMargeViewModelData.ItemListModel.VariantMasterId))
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "VariantType Change";
                            menuLog.ChangeItemName = previousVariantMasterName;
                            menuLog.OldValue = previousVariantMasterName;
                            menuLog.UpdatedValue = updatedVariantMasterName;
                            menuLog.ChangedObject = "Item";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();
                        }
                        item.VariantMasterId =Convert.ToInt32(itemMargeViewModelData.ItemListModel.VariantMasterId);

                        if (item.ItemName != itemMargeViewModelData.ItemListModel.ItemName)
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "Item Change";
                            menuLog.ChangeItemName = item.ItemName;
                            menuLog.OldValue = item.ItemName;
                            menuLog.UpdatedValue = itemMargeViewModelData.ItemListModel.ItemName;
                            menuLog.ChangedObject = "Item";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();
                        }
                        item.ItemName = itemMargeViewModelData.ItemListModel.ItemName;

                        if (item.ItemDescription != itemMargeViewModelData.ItemListModel.ItemDescription)
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "Description Change";
                            menuLog.ChangeItemName = item.ItemName;
                            menuLog.OldValue = item.ItemDescription;
                            menuLog.UpdatedValue = itemMargeViewModelData.ItemListModel.ItemDescription;
                            menuLog.ChangedObject = "Item";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();
                        }
                        item.ItemDescription = itemMargeViewModelData.ItemListModel.ItemDescription;

                        item.RestaurantCode = restaurantCode;
                        if (formFiles != null)
                        {
                            if (item.ItemImagePath != filePath)
                            {
                                MenuLog menuLog = new MenuLog();
                                menuLog.ChangeType = "Image Change";
                                menuLog.ChangeItemName = item.ItemName;
                                menuLog.OldValue = item.ItemImagePath;
                                menuLog.UpdatedValue = filePath;
                                menuLog.ChangedObject = "Item";
                                menuLog.RestaurantCode = restaurantCode;
                                menuLog.CreatedByName = userData.UserName;
                                menuLog.CreatedBy = loggedInUserId;
                                menuLog.CreatedDate = DateTime.UtcNow;
                                _DBContext.MenuLog.Add(menuLog);
                                _DBContext.SaveChanges();
                            }
                            item.ItemImagePath = filePath;
                        }
                        item.UpdatedByName = userData.UserName;
                        item.UpdatedBy = loggedInUserId;
                        item.UpdatedDate = DateTime.UtcNow;
                        _DBContext.SaveChanges();

                        string previousMenus = "";
                        string updatedMenus = "";

                        var itemWithMenuRemove = _DBContext.ItemWithMenu.Where(x => x.ItemId == item.ItemId && x.RestaurantCode==restaurantCode).ToList();
                        foreach (var data in itemWithMenuRemove)
                        {
                            var menus = _DBContext.Menu.Where(x => x.MenuId == data.MenuId).Select(x => x.MenuName).FirstOrDefault();
                            previousMenus = previousMenus + menus + ",";

                            _DBContext.ItemWithMenu.Remove(data);
                            _DBContext.SaveChanges();
                        }

                        foreach (var data in itemMargeViewModelData.ItemWithMenuModel)
                        {
                            var menus = _DBContext.Menu.Where(x => x.MenuId == data.MenuId).Select(x => x.MenuName).FirstOrDefault();
                            updatedMenus = updatedMenus + menus + ",";

                            var menu = _DBContext.Menu.Where(x => x.MenuId == data.MenuId).FirstOrDefault();
                            menu.UpdatedBy = loggedInUserId;
                            menu.UpdatedByName = userData.UserName;
                            menu.UpdatedDate = DateTime.UtcNow;
                            _DBContext.SaveChanges();

                            ItemWithMenu itemWithMenu = new ItemWithMenu();
                            itemWithMenu.ItemId = item.ItemId;
                            itemWithMenu.RestaurantCode = restaurantCode;
                            itemWithMenu.MenuId = data.MenuId;
                            itemWithMenu.CreatedByName = userData.UserName;
                            itemWithMenu.CreatedBy = loggedInUserId;
                            itemWithMenu.CreatedDate = DateTime.UtcNow;
                            _DBContext.ItemWithMenu.Add(itemWithMenu);
                            _DBContext.SaveChanges();
                        }

                        if (previousMenus != updatedMenus)
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "Item in Menu";
                            menuLog.ChangeItemName = item.ItemName;
                            menuLog.OldValue = previousMenus.Substring(0, previousMenus.Length - 1);
                            menuLog.UpdatedValue = updatedMenus.Substring(0, previousMenus.Length - 1);
                            menuLog.ChangedObject = "Item";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();
                        }

                        string previousCategory = "";
                        string updatedCategory = "";

                        var itemWithCategoryRemove = _DBContext.ItemWithCategory.Where(x => x.ItemId == item.ItemId && x.RestaurantCode==restaurantCode).ToList();
                        foreach (var data in itemWithCategoryRemove)
                        {
                            var categories = _DBContext.ItemCategory.Where(x => x.CategoryId == data.CategoryId).Select(x => x.CategoryName).FirstOrDefault();
                            previousCategory = previousCategory + categories + ",";

                            _DBContext.ItemWithCategory.Remove(data);
                            _DBContext.SaveChanges();
                        }

                        foreach (var data in itemMargeViewModelData.ItemWithCategoryModel)
                        {
                            var categories = _DBContext.ItemCategory.Where(x => x.CategoryId == data.CategoryId).Select(x => x.CategoryName).FirstOrDefault();
                            updatedCategory = updatedCategory + categories + ",";

                            ItemWithCategory itemWithCategory = new ItemWithCategory();
                            itemWithCategory.ItemId = item.ItemId;
                            itemWithCategory.RestaurantCode = restaurantCode;
                            itemWithCategory.CategoryId = data.CategoryId;
                            itemWithCategory.CreatedByName = userData.UserName;
                            itemWithCategory.CreatedBy = loggedInUserId;
                            itemWithCategory.CreatedDate = DateTime.UtcNow;
                            _DBContext.ItemWithCategory.Add(itemWithCategory);
                            _DBContext.SaveChanges();
                        }

                        if (previousCategory != updatedCategory)
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "Item in Category";
                            menuLog.ChangeItemName = item.ItemName;
                            menuLog.OldValue = previousCategory.Substring(0, previousCategory.Length - 1);
                            menuLog.UpdatedValue = updatedCategory.Substring(0, updatedCategory.Length - 1);
                            menuLog.ChangedObject = "Item";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();
                        }


                        var variantMasterId = await _DBContext.ItemVariant.Where(x => x.ItemId == item.ItemId && x.RestaurantCode == restaurantCode).Select(x=>x.VariantMasterId).FirstOrDefaultAsync();
                        var variantRemove = _DBContext.ItemVariant.Where(x => x.ItemId == item.ItemId && x.RestaurantCode == restaurantCode).ToList();

                        if (variantMasterId == item.VariantMasterId)
                        {
                            var result = itemMargeViewModelData.VariantPriceModel
                                        .Join(variantRemove, N => N.VariantId, P => P.VariantId, (N, P) => new { N, P })
                                        .Select(x=> new
                                        {
                                            PreviousPrice=x.P.VariantWiseItemprice,
                                            UpdatedPrice=x.N.VariantWiseItemprice
                                        }).ToList();

                            foreach(var data in result)
                            {
                                if (data.PreviousPrice != data.UpdatedPrice)
                                {
                                    MenuLog menuLog = new MenuLog();
                                    menuLog.ChangeType = "Price change";
                                    menuLog.ChangeItemName = item.ItemName;
                                    menuLog.OldValue = data.PreviousPrice.ToString();
                                    menuLog.UpdatedValue = data.UpdatedPrice.ToString();
                                    menuLog.ChangedObject = "Item";
                                    menuLog.RestaurantCode = restaurantCode;
                                    menuLog.CreatedByName = userData.UserName;
                                    menuLog.CreatedBy = loggedInUserId;
                                    menuLog.CreatedDate = DateTime.UtcNow;
                                    _DBContext.MenuLog.Add(menuLog);
                                    _DBContext.SaveChanges();
                                }
                            }
                        }
                        
                        foreach (var data in variantRemove)
                        {
                            _DBContext.ItemVariant.Remove(data);
                            _DBContext.SaveChanges();
                        }

                        foreach (var data in itemMargeViewModelData.VariantPriceModel)
                        {
                            ItemVariant itemVariant = new ItemVariant();
                            itemVariant.ItemId = item.ItemId;
                            itemVariant.VariantId = data.VariantId;
                            itemVariant.VariantWiseItemprice = data.VariantWiseItemprice;
                            itemVariant.VariantName = data.VariantName;
                            itemVariant.VariantMasterId = item.VariantMasterId;
                            itemVariant.RestaurantCode = restaurantCode;
                            itemVariant.CreatedByName = userData.UserName;
                            itemVariant.CreatedBy = loggedInUserId;
                            itemVariant.CreatedDate = DateTime.UtcNow;
                            _DBContext.ItemVariant.Add(itemVariant);
                            _DBContext.SaveChanges();
                        }

                        transaction.Commit();
                        return itemMargeViewModelData;
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

        public async Task<string> DeleteItemById(int loggedInUserId, string restaurantCode, int itemId, string code)
        {
            string actionName = "DeleteItemById";
            try
            {
                using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        var userData = await _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefaultAsync();
                        Item item = await _DBContext.Item.Where(x => x.ItemId == itemId && x.IsDeleted == false && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();
                        var verificationData = await _DBContext.VarificationCode.Where(x => x.Username == userData.UserName && x.IsDeleted == false && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                        if (verificationData.Code == code)
                        {
                            if (item != null)
                            {
                                MenuLog menuLog = new MenuLog();
                                menuLog.ChangeType = "Item Delete";
                                menuLog.ChangeItemName = item.ItemName;
                                menuLog.OldValue = "Active";
                                menuLog.UpdatedValue = "Deleted";
                                menuLog.ChangedObject = "Item";
                                menuLog.RestaurantCode = restaurantCode;
                                menuLog.CreatedByName = userData.UserName;
                                menuLog.CreatedBy = loggedInUserId;
                                menuLog.CreatedDate = DateTime.UtcNow;
                                _DBContext.MenuLog.Add(menuLog);
                                _DBContext.SaveChanges();

                                item.IsDeleted = true;
                                item.Status = false;
                                item.UpdatedBy = loggedInUserId;
                                item.UpdatedByName = userData.UserName;
                                item.UpdatedDate = DateTime.UtcNow;
                                _DBContext.SaveChanges();

                                var itemWithMenu = await _DBContext.ItemWithMenu.Where(x => x.ItemId == item.ItemId && x.RestaurantCode == restaurantCode).ToListAsync();
                                foreach (var data in itemWithMenu)
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

        public async Task<string> ChangeItemStatusById(int loggedInUserId, string restaurantCode, int itemId)
        {
            string actionName = "ChangeItemStatusById";
            try
            {
                using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();

                        Item item = await _DBContext.Item.Where(x => x.ItemId == itemId && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                        if (item != null)
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "Status Change";
                            menuLog.ChangeItemName = item.ItemName;
                            menuLog.OldValue = "Active";
                            menuLog.UpdatedValue = "Inactive";
                            menuLog.ChangedObject = "Item";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();

                            if (item.Status == true)
                            {
                                item.Status = false;
                            }
                            else
                            {
                                item.Status = true;
                            }
                            item.UpdatedBy = loggedInUserId;
                            item.UpdatedByName = userData.UserName;
                            item.UpdatedDate = DateTime.UtcNow;
                            _DBContext.SaveChanges();

                            var itemWithMenu = await _DBContext.ItemWithMenu.Where(x => x.ItemId == item.ItemId && x.RestaurantCode == restaurantCode).ToListAsync();
                            foreach (var data in itemWithMenu)
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


        public async Task<ItemMargeViewModel> GetItemList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetItemList";
            try
            {
                string sqlQuery = @"select ROW_NUMBER() Over (Order by I.ItemId desc) As Sl
                                        ,string_agg(M.MenuName, ',') as Menus
                                        ,string_agg(IC.CategoryName, ',') as Categories
                                        ,I.ItemId
                                        ,I.ItemName
                                        ,I.ItemDescription
                                        ,I.ItemImagePath
                                        ,I.RestaurantCode
                                        ,I.Status
                                        ,I.CreatedByName
										,VM.VariantMasterName
                                        ,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,I.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate
                                        ,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, I.UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                        from Item as I
                                        left join ItemWithMenu as IM on I.ItemId=IM.ItemId
                                        left join Menu as M on IM.MenuId=M.MenuId
										left join ItemWithCategory as IWC on I.ItemId=IWC.ItemId
                                        left join ItemCategory as IC on IWC.CategoryId=IC.CategoryId
										left join VariantMaster as VM on I.VariantMasterId=VM.VariantMasterId
                                        where I.RestaurantCode=@RestaurantCode And I.IsDeleted=0 
                                        group by I.ItemId,I.ItemName,I.ItemDescription,I.ItemImagePath,I.RestaurantCode,I.Status,I.CreatedByName
                                        ,I.CreatedDate,I.UpdatedDate,VM.VariantMasterName
                                        order by I.ItemId DESC";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    List<ItemListModel> itemListModeldata = new List<ItemListModel>();
                    var query = connection.Query<ItemListModel>(sqlQuery, new { RestaurantCode = restaurantCode }).ToList();
                    
                    foreach (var data in query)
                    {
                        itemListModeldata.Add(new ItemListModel
                        {
                            Sl = data.Sl,
                            Menus = data.Menus,
                            Categories = data.Categories,
                            ItemId = data.ItemId,
                            ItemName = data.ItemName,
                            ItemDescription = data.ItemDescription,
                            ItemImagePath = data.ItemImagePath,
                            RestaurantCode = data.RestaurantCode,
                            Status = data.Status,
                            CreatedByName = data.CreatedByName,
                            VariantMasterName = data.VariantMasterName,
                            CreatedDate = data.CreatedDate,
                            UpdatedDate=data.UpdatedDate,
                            VariantAndPriceModel = _DBContext.ItemVariant.Where(o => o.ItemId == data.ItemId).ToList()
                        });
                    }

                    ItemMargeViewModel itemMargeViewModel = new ItemMargeViewModel();
                    itemMargeViewModel.ItemListModels = itemListModeldata;
                    itemMargeViewModel.ItemListModel = new ItemListModel();
                    itemMargeViewModel.VariantAndPriceModel = new List<VariantAndPriceModel>();
                    itemMargeViewModel.ItemWithMenuModel = new List<ItemWithMenuModel>();
                    itemMargeViewModel.ItemWithCategoryModel = new List<ItemWithCategoryModel>();
                    itemMargeViewModel.VariantMaster = new List<VariantMaster>();
                    itemMargeViewModel.VariantPriceModel = new List<VariantPriceModel>();

                    return itemMargeViewModel;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<ItemMargeViewModel> GetItemListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetItemListBySearchValue";
            try
            {
                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    if (SearchText != null && SearchText != "")
                    {
                        string sqlQuery = @"select ROW_NUMBER() Over (Order by I.ItemId desc) As Sl
                                        ,string_agg(M.MenuName, ',') as Menus
                                        ,string_agg(IC.CategoryName, ',') as Categories
                                        ,I.ItemId
                                        ,I.ItemName
                                        ,I.ItemDescription
                                        ,I.ItemImagePath
                                        ,I.RestaurantCode
                                        ,I.Status
                                        ,I.CreatedByName
										,VM.VariantMasterName
                                        ,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,I.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate
                                        ,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, I.UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                        from Item as I
                                        left join ItemWithMenu as IM on I.ItemId=IM.ItemId
                                        left join Menu as M on IM.MenuId=M.MenuId
										left join ItemWithCategory as IWC on I.ItemId=IWC.ItemId
                                        left join ItemCategory as IC on IWC.CategoryId=IC.CategoryId
										left join VariantMaster as VM on I.VariantMasterId=VM.VariantMasterId
                                        where I.RestaurantCode=@RestaurantCode And I.IsDeleted=0 And I.ItemName=@ItemName
                                        group by I.ItemId,I.ItemName,I.ItemDescription,I.ItemImagePath,I.RestaurantCode,I.Status,I.CreatedByName
                                        ,I.CreatedDate,I.UpdatedDate,VM.VariantMasterName
                                        order by I.ItemId DESC";

                        List<ItemListModel> itemListModeldata = new List<ItemListModel>();
                        var query = connection.Query<ItemListModel>(sqlQuery, new { RestaurantCode = restaurantCode, ItemName=SearchText }).ToList();

                        foreach (var data in query)
                        {
                            itemListModeldata.Add(new ItemListModel
                            {
                                Sl = data.Sl,
                                Menus = data.Menus,
                                Categories = data.Categories,
                                ItemId = data.ItemId,
                                ItemName = data.ItemName,
                                ItemDescription = data.ItemDescription,
                                ItemImagePath = data.ItemImagePath,
                                RestaurantCode = data.RestaurantCode,
                                Status = data.Status,
                                CreatedByName = data.CreatedByName,
                                VariantMasterName = data.VariantMasterName,
                                CreatedDate = data.CreatedDate,
                                UpdatedDate = data.UpdatedDate,
                                VariantAndPriceModel = _DBContext.ItemVariant.Where(o => o.ItemId == data.ItemId).ToList()
                            });
                        }

                        ItemMargeViewModel itemMargeViewModel = new ItemMargeViewModel();
                        itemMargeViewModel.ItemListModels = itemListModeldata;
                        itemMargeViewModel.ItemListModel = new ItemListModel();
                        itemMargeViewModel.VariantAndPriceModel = new List<VariantAndPriceModel>();
                        itemMargeViewModel.ItemWithMenuModel = new List<ItemWithMenuModel>();
                        itemMargeViewModel.ItemWithCategoryModel = new List<ItemWithCategoryModel>();
                        itemMargeViewModel.VariantMaster = new List<VariantMaster>();
                        itemMargeViewModel.VariantPriceModel = new List<VariantPriceModel>();

                        return itemMargeViewModel;
                    }
                    else
                    {
                        string sqlQuery = @"select ROW_NUMBER() Over (Order by I.ItemId desc) As Sl
                                        ,string_agg(M.MenuName, ',') as Menus
                                        ,string_agg(IC.CategoryName, ',') as Categories
                                        ,I.ItemId
                                        ,I.ItemName
                                        ,I.ItemDescription
                                        ,I.ItemImagePath
                                        ,I.RestaurantCode
                                        ,I.Status
                                        ,I.CreatedByName
										,VM.VariantMasterName
                                        ,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,I.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate
                                        ,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, I.UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                        from Item as I
                                        left join ItemWithMenu as IM on I.ItemId=IM.ItemId
                                        left join Menu as M on IM.MenuId=M.MenuId
										left join ItemWithCategory as IWC on I.ItemId=IWC.ItemId
                                        left join ItemCategory as IC on IWC.CategoryId=IC.CategoryId
										left join VariantMaster as VM on I.VariantMasterId=VM.VariantMasterId
                                        where I.RestaurantCode=@RestaurantCode And I.IsDeleted=0
                                        group by I.ItemId,I.ItemName,I.ItemDescription,I.ItemImagePath,I.RestaurantCode,I.Status,I.CreatedByName
                                        ,I.CreatedDate,I.UpdatedDate,VM.VariantMasterName
                                        order by I.ItemId DESC";

                        List<ItemListModel> itemListModeldata = new List<ItemListModel>();
                        var query = connection.Query<ItemListModel>(sqlQuery, new { RestaurantCode = restaurantCode}).ToList();

                        foreach (var data in query)
                        {
                            itemListModeldata.Add(new ItemListModel
                            {
                                Sl = data.Sl,
                                Menus = data.Menus,
                                Categories = data.Categories,
                                ItemId = data.ItemId,
                                ItemName = data.ItemName,
                                ItemDescription = data.ItemDescription,
                                ItemImagePath = data.ItemImagePath,
                                RestaurantCode = data.RestaurantCode,
                                Status = data.Status,
                                CreatedByName = data.CreatedByName,
                                VariantMasterName = data.VariantMasterName,
                                CreatedDate = data.CreatedDate,
                                UpdatedDate = data.UpdatedDate,
                                VariantAndPriceModel = _DBContext.ItemVariant.Where(o => o.ItemId == data.ItemId).ToList()
                            });
                        }

                        ItemMargeViewModel itemMargeViewModel = new ItemMargeViewModel();
                        itemMargeViewModel.ItemListModels = itemListModeldata;
                        itemMargeViewModel.ItemListModel = new ItemListModel();
                        itemMargeViewModel.VariantAndPriceModel = new List<VariantAndPriceModel>();
                        itemMargeViewModel.ItemWithMenuModel = new List<ItemWithMenuModel>();
                        itemMargeViewModel.ItemWithCategoryModel = new List<ItemWithCategoryModel>();
                        itemMargeViewModel.VariantMaster = new List<VariantMaster>();
                        itemMargeViewModel.VariantPriceModel = new List<VariantPriceModel>();

                        return itemMargeViewModel;
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
