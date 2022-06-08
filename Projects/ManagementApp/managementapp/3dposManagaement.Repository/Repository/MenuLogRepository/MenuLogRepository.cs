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
using _3dposManagaement.Repository.IRepository.IMenuLogRepository;

namespace _3dposManagaement.Repository.Repository.MenuLogRepository
{
    public class MenuLogRepository : IMenuLogRepository
    {
        private const string _areaName = "3DPOSManagement Module";
        private const string _controllerName = "MenuLogRepository";
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;

        public MenuLogRepository(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
        }

        public async Task<List<MenuLogModel>> GetMenuLogList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetMenuLogList";
            try
            {
                string sqlQuery = @"select ROW_NUMBER() Over (Order by Id desc) As Sl,ChangeType,ChangeItemName,OldValue,UpdatedValue,ChangedObject,RestaurantCode,CreatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from MenuLog 
                                    where RestaurantCode='" + restaurantCode + "' order by Id DESC";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<MenuLogModel>(sqlQuery).ToList();

                    return query;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<MenuLogModel>> GetDateWiseMenuLogList(int loggedInUserId, string restaurantCode, string txtSelectedDateFrom, string txtSelectedDateTo)
        {
            string actionName = "GetMenuLogList";
            try
            {
                string sqlQuery = @"select ROW_NUMBER() Over (Order by Id desc) As Sl,ChangeType,ChangeItemName,OldValue,UpdatedValue,ChangedObject,RestaurantCode,CreatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from MenuLog 
                                    where RestaurantCode=@RestaurantCode and (CONVERT(date,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))))>=CONVERT(date,@txtSelectedDateFrom)) AND (CONVERT(date,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))))<=CONVERT(date,@txtSelectedDateTo)) order by Id DESC";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<MenuLogModel>(sqlQuery, new { RestaurantCode = restaurantCode, txtSelectedDateFrom = txtSelectedDateFrom, txtSelectedDateTo = txtSelectedDateTo }).ToList();

                    return query;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<MenuLogModel>> GetMenuLogListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetMenuLogListBySearchValue";
            try
            {
                List<MenuLog> menuLog = new List<MenuLog>();

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    if (SearchText != null && SearchText != "")
                    {
                        string sqlQuery = @"select ROW_NUMBER() Over (Order by Id desc) As Sl,ChangeType,ChangeItemName,UpdatedValue,ChangedObject,RestaurantCode,CreatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                            from MenuLog 
                                            where RestaurantCode=@RestaurantCode AND ChangeType like %@SearchText% OR ChangeItemName like %@SearchText% order by Id DESC";

                        var query = connection.Query<MenuLogModel>(sqlQuery, new { RestaurantCode = restaurantCode, SearchText = SearchText }).ToList();

                        return query;
                    }
                    else
                    {
                        string sqlQuery = @"select ROW_NUMBER() Over (Order by Id desc) As Sl,ChangeType,ChangeItemName,UpdatedValue,ChangedObject,RestaurantCode,CreatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                            from MenuLog 
                                            where RestaurantCode=@RestaurantCode order by Id DESC";

                        var query = connection.Query<MenuLogModel>(sqlQuery, new { RestaurantCode = restaurantCode}).ToList();

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
    }
}
