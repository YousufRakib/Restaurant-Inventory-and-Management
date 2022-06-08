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
using _3dposManagaement.Repository.IRepository.IVariantRepository;

namespace _3dposManagaement.Repository.Repository.VariantRepository
{
    public class VariantRepository : IVariantRepository
    {
        private const string _areaName = "3DPOSManagement Module";
        private const string _controllerName = "VariantRepository";
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;

        public VariantRepository(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
        }

        public async Task<VariantMargeViewModel> SaveVariant(VariantMargeViewModel variantMargeViewModel,int loggedInUserId, string restaurantCode)
        {
            string actionName = "SaveVariant";
            try
            {
                using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();

                        VariantMaster variantMaster = new VariantMaster();
                        variantMaster.VariantMasterName = variantMargeViewModel.VariantViewModel.VariantMasterName;
                        variantMaster.VariantMasterNumber = variantMargeViewModel.VariantViewModel.VariantMasterNumber;
                        variantMaster.RestaurantCode = restaurantCode;
                        variantMaster.IsDeleted = false;
                        variantMaster.Status = true;
                        variantMaster.CreatedByName = userData.UserName;
                        variantMaster.CreatedBy = loggedInUserId;
                        variantMaster.CreatedDate = DateTime.UtcNow;

                        _DBContext.VariantMaster.Add(variantMaster);
                        _DBContext.SaveChanges();

                        foreach (var data in variantMargeViewModel.VariantViewModelList)
                        {
                            Variant variant = new Variant();
                            variant.VariantName = data.VariantName;
                            variant.RestaurantCode = restaurantCode;
                            variant.VariantMasterId= variantMaster.VariantMasterId;
                            variant.IsDeleted = false;
                            variant.Status = true;
                            variant.CreatedByName = userData.UserName;
                            variant.CreatedBy = loggedInUserId;
                            variant.CreatedDate = DateTime.UtcNow;
                            _DBContext.Variant.Add(variant);
                            _DBContext.SaveChanges();
                        }

                        transaction.Commit();
                        return variantMargeViewModel;
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

        public async Task<VariantMargeViewModel> GetVariantList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetVariantList";
            try
            {
                string sqlQuery = @"Select ROW_NUMBER() Over (Order by VM.VariantMasterId desc) As Sl
                                    ,VM.VariantMasterId
                                    ,VM.VariantMasterName
                                    ,VM.VariantMasterNumber
                                    ,VM.Status
                                    ,string_agg(V.VariantName, ',') as VariantName
                                    from VariantMaster as VM 
                                    left join Variant as V on VM.VariantMasterId=V.VariantMasterId
                                    where VM.RestaurantCode=@RestaurantCode And VM.IsDeleted=0 
                                    group by VM.VariantMasterId,VM.VariantMasterName,VM.VariantMasterNumber,VM.Status
                                    order by VM.VariantMasterId DESC";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<VariantViewModel>(sqlQuery, new { RestaurantCode = restaurantCode }).ToList();

                    VariantMargeViewModel variantMargeViewModel = new VariantMargeViewModel();
                    variantMargeViewModel.VariantViewModelList = query;
                    variantMargeViewModel.VariantViewModel = new VariantViewModel();

                    return variantMargeViewModel;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<VariantMargeViewModel> GetVariantListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetVariantListBySearchValue";
            try
            {
                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    if (SearchText != null && SearchText != "")
                    {
                        string sqlQuery = @"Select ROW_NUMBER() Over (Order by VM.VariantMasterId desc) As Sl
                                    ,VM.VariantMasterId
                                    ,VM.VariantMasterName
                                    ,VM.VariantMasterNumber
                                    ,VM.Status
                                    ,string_agg(V.VariantName, ',') as VariantName
                                    from VariantMaster as VM 
                                    left join Variant as V on VM.VariantMasterId=V.VariantMasterId
                                    where VM.RestaurantCode=@RestaurantCode And VM.IsDeleted=0 And I.VariantMasterName=@VariantMasterName
                                    group by VM.VariantMasterId,VM.VariantMasterName,VM.VariantMasterNymber,VM.Status
                                    order by VM.VariantMasterId DESC";

                        var query = connection.Query<VariantViewModel>(sqlQuery, new { RestaurantCode = restaurantCode, VariantMasterName = SearchText }).ToList();

                        VariantMargeViewModel variantMargeViewModel = new VariantMargeViewModel();
                        variantMargeViewModel.VariantViewModelList = query;
                        variantMargeViewModel.VariantViewModel = new VariantViewModel();

                        return variantMargeViewModel;
                    }
                    else
                    {
                        string sqlQuery = @"Select ROW_NUMBER() Over (Order by VM.VariantMasterId desc) As Sl
                                    ,VM.VariantMasterId
                                    ,VM.VariantMasterName
                                    ,VM.VariantMasterNumber
                                    ,VM.Status
                                    ,string_agg(V.VariantName, ',') as VariantName
                                    from VariantMaster as VM 
                                    left join Variant as V on VM.VariantMasterId=V.VariantMasterId
                                    where VM.RestaurantCode=@RestaurantCode And VM.IsDeleted=0 
                                    group by VM.VariantMasterId,VM.VariantMasterName,VM.VariantMasterNymber,VM.Status
                                    order by VM.VariantMasterId DESC";

                        var query = connection.Query<VariantViewModel>(sqlQuery, new { RestaurantCode = restaurantCode}).ToList();

                        VariantMargeViewModel variantMargeViewModel = new VariantMargeViewModel();
                        variantMargeViewModel.VariantViewModelList = query;
                        variantMargeViewModel.VariantViewModel = new VariantViewModel();

                        return variantMargeViewModel;
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<string> ChangeVariantStatusById(int loggedInUserId, string restaurantCode, int variantId)
        {
            string actionName = "ChangeVariantStatusById";
            try
            {
                using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();

                        VariantMaster variantMaster = await _DBContext.VariantMaster.Where(x => x.VariantMasterId == variantId && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                        if (variantMaster != null)
                        {
                            MenuLog menuLog = new MenuLog();
                            menuLog.ChangeType = "Status Change";
                            menuLog.ChangeItemName = variantMaster.VariantMasterName;
                            menuLog.OldValue = "Active";
                            menuLog.UpdatedValue = "Inactive";
                            menuLog.ChangedObject = "VariantMaster";
                            menuLog.RestaurantCode = restaurantCode;
                            menuLog.CreatedByName = userData.UserName;
                            menuLog.CreatedBy = loggedInUserId;
                            menuLog.CreatedDate = DateTime.UtcNow;
                            _DBContext.MenuLog.Add(menuLog);
                            _DBContext.SaveChanges();

                            if (variantMaster.Status == true)
                            {
                                variantMaster.Status = false;
                            }
                            else
                            {
                                variantMaster.Status = true;
                            }
                            variantMaster.UpdatedBy = loggedInUserId;
                            variantMaster.UpdatedByName = userData.UserName;
                            variantMaster.UpdatedDate = DateTime.UtcNow;
                            _DBContext.SaveChanges();

                            var item = await _DBContext.Item.Where(x => x.VariantMasterId == variantMaster.VariantMasterId && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();
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
    }
}
