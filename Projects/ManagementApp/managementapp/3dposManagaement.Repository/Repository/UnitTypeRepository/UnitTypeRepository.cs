using _3dposManagaement.Database;
using _3dposManagaement.Repository.IRepository.ILogRepository;
using _3dposManagaement.Repository.IRepository.IUnitTypeRepository;
using _3dposManagaement.Utility.CommonModel;
using CommonEntityModel.EntityModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
//using _3DPOSRegistrationApp.Database;

namespace _3dposManagaement.Repository.Repository.UnitTypeRepository
{
    public class UnitTypeRepository : IUnitTypeRepository
    {
        private const string _areaName = "3DPOSManagement Module";
        private const string _controllerName = "UnitTypeRepository";
        private readonly IConfiguration _configuration;
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;

        public UnitTypeRepository(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository, IConfiguration configuration)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._configuration = configuration;
        }

        public UnitType SaveUnitType(UnitType unitType, int loggedInUserId, string restaurantCode)
        {
            string actionName = "SaveUnitType";
            try
            {
                var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();

                UnitType unitTypeData = new UnitType();

                unitTypeData.UnitTypeName = unitType.UnitTypeName;
                unitTypeData.RestaurantCode = restaurantCode;
                unitTypeData.CreatedBy = loggedInUserId;
                unitTypeData.CreatedDate = DateTime.UtcNow;

                _DBContext.UnitType.Add(unitTypeData);
                _DBContext.SaveChanges();

                return unitTypeData;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<string> CheckExistUniteType(UnitType unitType, int loggedInUserId, string restaurantCode)
        {
            string actionName = "SaveUnitType";
            try
            {
                var existUniteType = _DBContext.UnitType.Where(x => x.RestaurantCode == unitType.RestaurantCode && x.UnitTypeName == unitType.UnitTypeName).FirstOrDefault();

                if (existUniteType != null)
                {
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

        public async Task<List<UniteTypeViewModel>> GetUnitTypeList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetUnitTypeList";
            try
            {
                List<UniteTypeViewModel> unitTypeList = new List<UniteTypeViewModel>();
                DataTable dt = new DataTable();

                string connString = _configuration.GetConnectionString("RestaurantDBConnection");
                //Resturant resturant = _3DPOSDBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();

                string sqlQuery = @"select UT.UnitTypeId,UT.UnitTypeName, U1.UserName as CreatedBy,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UT.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate
                                    ,U2.UserName as UpdatedBy, CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UT.UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from UnitType as UT
                                    left join AspNetUsers as U1 on UT.CreatedBy=U1.Id
                                    left join AspNetUsers as U2 on UT.UpdatedBy=U2.Id
                                    where UT.RestaurantCode='" + restaurantCode + "' order by UT.UnitTypeId DESC";

                using (SqlConnection connection = new SqlConnection(connString))
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlQuery, connection))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            connection.Open();
                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            da.Fill(dt);
                        }

                        foreach (DataRow row in dt.Rows)
                        {
                            UniteTypeViewModel uniteTypeViewModel = new UniteTypeViewModel();

                            uniteTypeViewModel.UnitTypeId = Convert.ToInt32(row["UnitTypeId"].ToString());
                            uniteTypeViewModel.UnitTypeName = row["UnitTypeName"].ToString();
                            uniteTypeViewModel.CreatedByName = row["CreatedBy"].ToString();
                            uniteTypeViewModel.CreatedDate = row["CreatedDate"].ToString();
                            uniteTypeViewModel.UpdatedByName = row["UpdatedBy"].ToString();
                            uniteTypeViewModel.UpdatedDate = row["UpdatedDate"].ToString();

                            unitTypeList.Add(uniteTypeViewModel);
                        }
                    }
                    catch (Exception ex)
                    {
                        connection.Close();
                        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                        return null;
                    }
                    connection.Close();
                }
                return unitTypeList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<UniteTypeViewModel>> GetUnitTypeListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetUnitTypeListBySearchValue";
            try
            {
                List<UniteTypeViewModel> unitTypeList = new List<UniteTypeViewModel>();
                DataTable dt = new DataTable();

                string connString = _configuration.GetConnectionString("RestaurantDBConnection");
                //Resturant resturant = _3DPOSDBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();

                bool isInt = SearchText.All(char.IsDigit);

                if (isInt == true)
                {
                    string sqlQuery = @"select UT.UnitTypeId,UT.UnitTypeName, U1.UserName as CreatedBy,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UT.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate
                                    ,U2.UserName as UpdatedBy,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, UT.UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from UnitType as UT
                                    left join AspNetUsers as U1 on UT.CreatedBy=U1.Id
                                    left join AspNetUsers as U2 on UT.UpdatedBy=U2.Id
                                    where UT.UnitTypeName like '%" + SearchText + "%' OR UT.UnitTypeId=" + SearchText + " AND UT.RestaurantCode='" + restaurantCode + "' order by UT.UnitTypeId DESC";

                    using (SqlConnection connection = new SqlConnection(connString))
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand(sqlQuery, connection))
                            {
                                cmd.CommandTimeout = 0;
                                cmd.CommandType = CommandType.Text;
                                connection.Open();
                                SqlDataAdapter da = new SqlDataAdapter(cmd);
                                da.Fill(dt);
                            }

                            foreach (DataRow row in dt.Rows)
                            {
                                UniteTypeViewModel uniteTypeViewModel = new UniteTypeViewModel();

                                uniteTypeViewModel.UnitTypeId = Convert.ToInt32(row["UnitTypeId"].ToString());
                                uniteTypeViewModel.UnitTypeName = row["UnitTypeName"].ToString();
                                uniteTypeViewModel.CreatedByName = row["CreatedBy"].ToString();
                                uniteTypeViewModel.CreatedDate = row["CreatedDate"].ToString();
                                uniteTypeViewModel.UpdatedByName = row["UpdatedBy"].ToString();
                                uniteTypeViewModel.UpdatedDate = row["UpdatedDate"].ToString();

                                unitTypeList.Add(uniteTypeViewModel);
                            }
                        }
                        catch (Exception ex)
                        {
                            connection.Close();
                            bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                            return null;
                        }
                        connection.Close();
                    }
                    return unitTypeList;
                }
                else
                {
                    string sqlQuery = @"select UT.UnitTypeId,UT.UnitTypeName, U1.UserName as CreatedBy,UT.CreatedDate
                                    ,U2.UserName as UpdatedBy,UT.UpdatedDate
                                    from UnitType as UT
                                    left join AspNetUsers as U1 on UT.CreatedBy=U1.Id
                                    left join AspNetUsers as U2 on UT.UpdatedBy=U2.Id
                                    where UT.UnitTypeName like '%" + SearchText + "%' AND UT.RestaurantCode='" + restaurantCode + "' order by UT.UnitTypeId DESC";

                    using (SqlConnection connection = new SqlConnection(connString))
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand(sqlQuery, connection))
                            {
                                cmd.CommandTimeout = 0;
                                cmd.CommandType = CommandType.Text;
                                connection.Open();
                                SqlDataAdapter da = new SqlDataAdapter(cmd);
                                da.Fill(dt);
                            }

                            foreach (DataRow row in dt.Rows)
                            {
                                UniteTypeViewModel uniteTypeViewModel = new UniteTypeViewModel();

                                uniteTypeViewModel.UnitTypeId = Convert.ToInt32(row["UnitTypeId"].ToString());
                                uniteTypeViewModel.UnitTypeName = row["UnitTypeName"].ToString();
                                uniteTypeViewModel.CreatedByName = row["CreatedBy"].ToString();
                                uniteTypeViewModel.CreatedDate = row["CreatedDate"].ToString();
                                uniteTypeViewModel.UpdatedByName = row["UpdatedBy"].ToString();
                                uniteTypeViewModel.UpdatedDate = row["UpdatedDate"].ToString();

                                unitTypeList.Add(uniteTypeViewModel);
                            }
                        }
                        catch (Exception ex)
                        {
                            connection.Close();
                            bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                            return null;
                        }
                        connection.Close();
                    }
                    return unitTypeList;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<UnitType> GetUniteTypeByID(int id, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetUniteTypeByID";
            UnitType unitType = new UnitType();

            try
            {
                var unitTypedata = await _DBContext.UnitType.Where(x => x.UnitTypeId == id && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                unitType.UnitTypeId = unitTypedata.UnitTypeId;
                unitType.UnitTypeName = unitTypedata.UnitTypeName;

                return unitType;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public UnitType EditUnitType(UnitType unitType, int loggedInUserId, string restaurantCode)
        {
            string actionName = "EditUnitType";
            try
            {
                var existUnitType = _DBContext.UnitType.Where(x => x.UnitTypeId == unitType.UnitTypeId && x.RestaurantCode == restaurantCode).FirstOrDefault();

                existUnitType.UnitTypeName = unitType.UnitTypeName;
                existUnitType.UpdatedBy = loggedInUserId;
                existUnitType.UpdatedDate = DateTime.UtcNow;

                _DBContext.SaveChanges();

                return existUnitType;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<string> RemoveUnitType(int unitTypeId,string code, int loggedInUserId, string restaurantCode)
        {
            string actionName = "RemoveUnitType";

            try
            {
                var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();
                var verificationData = await _DBContext.VarificationCode.Where(x => x.Username == userData.UserName && x.IsDeleted == false && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                if (verificationData.Code == code)
                {
                    var checkUnitTypeInProduct = await _DBContext.Product.Where(x => x.UnitTypeId == unitTypeId && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();
                    if (checkUnitTypeInProduct == null)
                    {
                        var unitTypeData = await _DBContext.UnitType.Where(x => x.UnitTypeId == unitTypeId && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();
                        
                        _DBContext.UnitType.Remove(unitTypeData);
                        _DBContext.SaveChanges();

                        return "true";
                    }
                    else
                    {
                        return "exist";
                    }
                }
                else
                {
                    return "false";
                }

            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return "tryCatch";
            }
        }
    }
}
