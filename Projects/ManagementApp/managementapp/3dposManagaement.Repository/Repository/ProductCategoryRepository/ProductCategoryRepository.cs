using _3dposManagaement.Database;
using _3dposManagaement.Repository.IRepository.ILogRepository;
using _3dposManagaement.Repository.IRepository.IProductCategoryRepository;
using _3dposManagaement.Utility.CommonModel;
using Microsoft.Extensions.Configuration;
using CommonEntityModel.EntityModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.Repository.ProductCategoryRepository
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private const string _areaName = "3DPOSManagement Module";
        private const string _controllerName = "ProductCategoryRepository";
        private readonly IConfiguration _configuration;
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;

        public ProductCategoryRepository(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository, IConfiguration configuration)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._configuration = configuration;
        }

        public ProductCategory SaveProductCategory(ProductCategory productCategory, int loggedInUserId, string restaurantCode)
        {
            string actionName = "SaveProductCategory";
            try
            {
                var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();

                ProductCategory productCategoryData = new ProductCategory();

                productCategoryData.ProductCategoryName = productCategory.ProductCategoryName;
                productCategoryData.RestaurantCode = restaurantCode;
                productCategoryData.CreatedBy = loggedInUserId;
                productCategoryData.CreatedDate = DateTime.UtcNow;

                _DBContext.ProductCategory.Add(productCategoryData);
                _DBContext.SaveChanges();

                return productCategoryData;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<string> CheckExistProductCategory(ProductCategory productCategory, int loggedInUserId, string restaurantCode)
        {
            string actionName = "CheckExistProductCategory";
            try
            {
                var existProductCategory = _DBContext.ProductCategory.Where(x => x.RestaurantCode == productCategory.RestaurantCode && x.ProductCategoryName == productCategory.ProductCategoryName).FirstOrDefault();

                if (existProductCategory != null)
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

        public async Task<List<ProductCategoryViewModel>> GetProductCategory(int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetProductCategory";
            try
            {
                List<ProductCategoryViewModel> productCategoryList = new List<ProductCategoryViewModel>();

                DataTable dt = new DataTable();

                string connString = _configuration.GetConnectionString("RestaurantDBConnection");
                //Resturant resturant = _3DPOSDBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();

                string sqlQuery = @"select PC.ProductCategoryId,PC.ProductCategoryName, U1.UserName as CreatedBy,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, PC.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,U2.UserName as UpdatedBy,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, PC.UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from ProductCategory as PC
                                    left join AspNetUsers as U1 on PC.CreatedBy=U1.Id
                                    left join AspNetUsers as U2 on PC.UpdatedBy=U2.Id
                                    where PC.RestaurantCode='" + restaurantCode + "' order by PC.ProductCategoryId DESC";

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
                            ProductCategoryViewModel productCategoryViewModel = new ProductCategoryViewModel();

                            productCategoryViewModel.ProductCategoryId = Convert.ToInt32(row["ProductCategoryId"].ToString());
                            productCategoryViewModel.ProductCategoryName = row["ProductCategoryName"].ToString();
                            productCategoryViewModel.CreatedByName = row["CreatedBy"].ToString();
                            productCategoryViewModel.CreatedDate = row["CreatedDate"].ToString();
                            productCategoryViewModel.UpdatedByName = row["UpdatedBy"].ToString();
                            productCategoryViewModel.UpdatedDate = row["UpdatedDate"].ToString();

                            productCategoryList.Add(productCategoryViewModel);
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
                return productCategoryList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<ProductCategoryViewModel>> GetProductCategoryBySearchValue(string SearchText, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetProductCategoryBySearchValue";
            try
            {
                List<ProductCategoryViewModel> productCategoryList = new List<ProductCategoryViewModel>();

                DataTable dt = new DataTable();

                //string connString = _configuration.GetConnectionString("RestaurantDBConnection");
                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();

                bool isInt = SearchText.All(char.IsDigit);

                if (isInt == true)
                {
                    string sqlQuery = @"select PC.ProductCategoryId,PC.ProductCategoryName, U1.UserName as CreatedBy,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, PC.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,U2.UserName as UpdatedBy,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, PC.UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                        from ProductCategory as PC
                                        left join AspNetUsers as U1 on PC.CreatedBy=U1.Id
                                        left join AspNetUsers as U2 on PC.UpdatedBy=U2.Id
                                        where PC.ProductCategoryName like '%" + SearchText + "%' OR PC.ProductCategoryId=" + SearchText + " AND PC.RestaurantCode='" + restaurantCode + "' order by PC.ProductCategoryId DESC";

                    using (SqlConnection connection = new SqlConnection(resturant.DBConnectionString))
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
                                ProductCategoryViewModel productCategoryViewModel = new ProductCategoryViewModel();

                                productCategoryViewModel.ProductCategoryId = Convert.ToInt32(row["ProductCategoryId"].ToString());
                                productCategoryViewModel.ProductCategoryName = row["ProductCategoryName"].ToString();
                                productCategoryViewModel.CreatedByName = row["CreatedBy"].ToString();
                                productCategoryViewModel.CreatedDate = row["CreatedDate"].ToString();
                                productCategoryViewModel.UpdatedByName = row["UpdatedBy"].ToString();
                                productCategoryViewModel.UpdatedDate = row["UpdatedDate"].ToString();

                                productCategoryList.Add(productCategoryViewModel);
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
                    return productCategoryList;
                }
                else
                {
                    string sqlQuery = @"select PC.ProductCategoryId,PC.ProductCategoryName, U1.UserName as CreatedBy,PC.CreatedDate,U2.UserName as UpdatedBy,PC.UpdatedDate
                                        from ProductCategory as PC
                                        left join AspNetUsers as U1 on PC.CreatedBy=U1.Id
                                        left join AspNetUsers as U2 on PC.UpdatedBy=U2.Id
                                        where PC.ProductCategoryName like '%" + SearchText + "%' AND PC.RestaurantCode='" + restaurantCode + "' order by PC.ProductCategoryId DESC";

                    using (SqlConnection connection = new SqlConnection(resturant.DBConnectionString))
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
                                ProductCategoryViewModel productCategoryViewModel = new ProductCategoryViewModel();

                                productCategoryViewModel.ProductCategoryId = Convert.ToInt32(row["ProductCategoryId"].ToString());
                                productCategoryViewModel.ProductCategoryName = row["ProductCategoryName"].ToString();
                                productCategoryViewModel.CreatedByName = row["CreatedBy"].ToString();
                                productCategoryViewModel.CreatedDate = row["CreatedDate"].ToString();
                                productCategoryViewModel.UpdatedByName = row["UpdatedBy"].ToString();
                                productCategoryViewModel.UpdatedDate = row["UpdatedDate"].ToString();

                                productCategoryList.Add(productCategoryViewModel);
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
                    return productCategoryList;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<ProductCategory> GetProductCategoryByID(int id, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetProductCategoryByID";
            ProductCategory productCategory = new ProductCategory();

            try
            {
                var productCategorydata = await _DBContext.ProductCategory.Where(x => x.ProductCategoryId == id && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                productCategory.ProductCategoryId = productCategorydata.ProductCategoryId;
                productCategory.ProductCategoryName = productCategorydata.ProductCategoryName;

                return productCategory;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public ProductCategory EditProductCategory(ProductCategory productCategory, int loggedInUserId, string restaurantCode)
        {
            string actionName = "EditProductCategory";
            try
            {
                var existProductCategory = _DBContext.ProductCategory.Where(x => x.ProductCategoryId == productCategory.ProductCategoryId && x.RestaurantCode == restaurantCode).FirstOrDefault();

                existProductCategory.ProductCategoryName = productCategory.ProductCategoryName;
                existProductCategory.UpdatedBy = loggedInUserId;
                existProductCategory.UpdatedDate = DateTime.UtcNow;

                _DBContext.SaveChanges();

                return existProductCategory;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<string> RemoveProductCategory(int productCategoryId,string code, int loggedInUserId, string restaurantCode)
        {
            string actionName = "RemoveProductCategory";

            try
            {
                var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();
                var verificationData = await _DBContext.VarificationCode.Where(x => x.Username == userData.UserName && x.IsDeleted == false && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                if (verificationData.Code == code)
                {
                    var checkProductCategoryInProduct = await _DBContext.Product.Where(x => x.ProductCategoryId == productCategoryId && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();
                    if (checkProductCategoryInProduct == null)
                    {
                        var productCategoryData = await _DBContext.ProductCategory.Where(x => x.ProductCategoryId == productCategoryId && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                        _DBContext.ProductCategory.Remove(productCategoryData);
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
