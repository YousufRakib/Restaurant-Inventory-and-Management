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
using _3dposManagaement.Repository.IRepository.IVarificationCodeRepository;

namespace _3dposManagaement.Repository.Repository.MenuRepository
{
    public class VarificationCodeRepository : IVarificationCodeRepository
    {
        private const string _areaName = "3DPOSManagement Module";
        private const string _controllerName = "VarificationCodeRepository";
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IConfiguration _configuration;
        private IHostingEnvironment hostingEnv;

        public VarificationCodeRepository(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository, IConfiguration configuration, IHostingEnvironment env)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._configuration = configuration;
            this.hostingEnv = env;
        }

        public async Task<List<VarificationCodeUserModel>> NotExistUserInVarificationCode(int loggedInUserId, string restaurantCode)
        {
            string actionName = "NotExistUserInVarificationCode";
            try
            {
                List<VarificationCodeUserModel> varificationCodeUserModelList = new List<VarificationCodeUserModel>();
                var data2 = _DBContext.Users.ToList();
                var notExistUserInVarificationCode =await
                    (from U in _DBContext.Users
                     from V in _DBContext.VarificationCode.Where(V => V.Username == U.UserName).DefaultIfEmpty()
                     select new 
                     {
                        U.Id,
                        U.UserName,
                        U.Email,
                        V.Username
                     })
                     .Where(x => x.Username==null)
                     .ToListAsync();

                foreach(var data in notExistUserInVarificationCode)
                {
                    VarificationCodeUserModel varificationCodeUserModel = new VarificationCodeUserModel();

                    varificationCodeUserModel.Id = data.Id;
                    varificationCodeUserModel.Username = data.UserName;
                    varificationCodeUserModel.Email = data.Email;
                    varificationCodeUserModelList.Add(varificationCodeUserModel);
                }

                return varificationCodeUserModelList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<VarificationCodeViewModel> SaveVarificationCode(VarificationCodeViewModel varificationCodeData, int loggedInUserId, string restaurantCode)
        {
            string actionName = "SaveVarificationCode";
            try
            {
                var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();


                VarificationCode varificationCode = new VarificationCode();

                varificationCode.Code = varificationCodeData.Code;
                varificationCode.Username = varificationCodeData.Username;
                varificationCode.RestaurantCode = restaurantCode;
                varificationCode.IsDeleted = false;
                varificationCode.Status = true;
                varificationCode.CreatedByName = userData.UserName;
                varificationCode.CreatedBy = loggedInUserId;
                varificationCode.CreatedDate = DateTime.UtcNow;

                _DBContext.VarificationCode.Add(varificationCode);
                _DBContext.SaveChanges();

                return varificationCodeData;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<VarificationCodeViewModel>> GetVarificationCodeList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetVarificationCodeList";
            try
            {
                string sqlQuery = @"select ROW_NUMBER() Over (Order by Id desc) As SI,Id,Code,Username, RestaurantCode,Status,CreatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from VarificationCode 
                                    where RestaurantCode='" + restaurantCode + "' order by Id DESC";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<VarificationCodeViewModel>(sqlQuery).ToList();

                    return query;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<VarificationCodeViewModel>> GetVarificationCodeListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetVarificationCodeListBySearchValue";
            try
            {
                string sqlQuery = "";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    if (SearchText != null && SearchText != "")
                    {
                        sqlQuery = @"select ROW_NUMBER() Over (Order by Id desc) As SI,Id,Code,Username, RestaurantCode,Status,CreatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from VarificationCode 
                                    where RestaurantCode=@RestaurantCode And Username=@Username And Code=@Code order by Id DESC";

                        var query = connection.Query<VarificationCodeViewModel>(sqlQuery, new { RestaurantCode = restaurantCode, Username = SearchText, Code = SearchText }).ToList();

                        return query;
                    }
                    else
                    {
                        sqlQuery = @"select ROW_NUMBER() Over (Order by Id desc) As SI,Id,Code,Username, RestaurantCode,Status,CreatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET,CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from VarificationCode 
                                    where RestaurantCode=@RestaurantCode  order by Id DESC";

                        var query = connection.Query<VarificationCodeViewModel>(sqlQuery, new { RestaurantCode = restaurantCode }).ToList();

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

        public async Task<VarificationCodeViewModel> GetVarificationCodeById(int loggedInUserId, string restaurantCode, int varificationCodeId)
        {
            string actionName = "GetVarificationCodeById";
            try
            {
                VarificationCodeViewModel varificationCodeViewModel = new VarificationCodeViewModel();
                var varificationCodeViewModelData = await _DBContext.VarificationCode.Where(x => x.Id == varificationCodeId && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                varificationCodeViewModel.Id = varificationCodeViewModelData.Id;
                varificationCodeViewModel.Code = varificationCodeViewModelData.Code;
                varificationCodeViewModel.Username = varificationCodeViewModelData.Username;

                return varificationCodeViewModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<string> DeleteVarificationCodeById(int loggedInUserId, string restaurantCode, int varificationCodeId,string code)
        {
            string actionName = "DeleteVarificationCodeById";
            try
            {
                var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();
                var verificationData = await _DBContext.VarificationCode.Where(x => x.Username == userData.UserName && x.IsDeleted == false && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                if (verificationData.Code == code)
                {
                    var verificationCode = _DBContext.VarificationCode.Where(x => x.Id == varificationCodeId).FirstOrDefault();

                    _DBContext.Remove(verificationCode);
                    _DBContext.SaveChanges();

                    return "True";
                }
                else
                {
                    return "False";
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return "Error";
            }
        }

        public async Task<VarificationCodeViewModel> UpdateVarificationCode(VarificationCodeViewModel varificationCodeData, int loggedInUserId, string restaurantCode)
        {
            string actionName = "UpdateVarificationCode";
            try
            {
                var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();

                var verificationCode = _DBContext.VarificationCode.Where(x => x.Id == varificationCodeData.Id).FirstOrDefault();
                verificationCode.Code = varificationCodeData.Code;
                verificationCode.UpdatedByName = userData.UserName;
                verificationCode.UpdatedBy = loggedInUserId;
                verificationCode.UpdatedDate = DateTime.UtcNow;

                _DBContext.SaveChanges();

                return varificationCodeData;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }
    }
}
