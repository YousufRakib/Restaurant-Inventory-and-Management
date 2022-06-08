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

namespace _3dposManagaement.Repository.Repository.ProductRepository
{
    public class ProductRepository : IProductRepository
    {
        private const string _areaName = "3DPOSManagement Module";
        private const string _controllerName = "ProductRepository";
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IConfiguration _configuration;

        public ProductRepository(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository, IConfiguration configuration)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._configuration = configuration;
        }

        public Product SaveProduct(Product product, int loggedInUserId, string restaurantCode)
        {
            string actionName = "SaveProduct";
            try
            {
                var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();

                Product productData = new Product();

                productData.ProductName = product.ProductName;
                productData.ProductCategoryId = product.ProductCategoryId;
                productData.RestaurantCode = restaurantCode;
                productData.UnitTypeId = product.UnitTypeId;
                productData.CreatedByName = userData.UserName;
                productData.CreatedBy = loggedInUserId;
                productData.CreatedDate = DateTime.UtcNow;

                _DBContext.Product.Add(productData);
                _DBContext.SaveChanges();

                return productData;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<string> CheckExistProduct(Product product, int loggedInUserId, string restaurantCode)
        {
            string actionName = "CheckExistProduct";
            try
            {
                var existProduct = _DBContext.Product.Where(x => x.RestaurantCode == product.RestaurantCode && x.ProductName == product.ProductName).FirstOrDefault();

                if (existProduct != null)
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

        public async Task<List<ProductModel>> GetProductList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetProductList";
            try
            {
                List<ProductModel> productList = new List<ProductModel>();

                DataTable dt = new DataTable();

                //string connString = _configuration.GetConnectionString("RestaurantDBConnection");
                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();

                string sqlQuery = @"select P.ProductId,P.ProductName,PC.ProductCategoryName,UT.UnitTypeName, P.CreatedByName as CreatedBy,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, P.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,P.UpdatedByName as UpdatedBy,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, P.UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from Product as P
                                    left join UnitType as UT on P.UnitTypeId=UT.UnitTypeId
                                    left join ProductCategory as PC on P.ProductCategoryId=PC.ProductCategoryId
                                    where P.RestaurantCode='" + restaurantCode + "' order by P.ProductId DESC";

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
                            ProductModel productModel = new ProductModel();

                            productModel.ProductId = Convert.ToInt32(row["ProductId"].ToString());
                            productModel.ProductName = row["ProductName"].ToString();
                            productModel.UnitType = row["UnitTypeName"].ToString();
                            productModel.ProductCategory = row["ProductCategoryName"].ToString();
                            productModel.CreatedByName = row["CreatedBy"].ToString();
                            productModel.CreatedDate = row["CreatedDate"].ToString();
                            productModel.UpdatedByName = row["UpdatedBy"].ToString();
                            productModel.UpdatedDate = row["UpdatedDate"].ToString();

                            productList.Add(productModel);
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

                return productList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<ProductModel>> GetProductListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetProductListBySearchValue";
            try
            {
                List<ProductModel> productList = new List<ProductModel>();

                bool isInt = SearchText.All(char.IsDigit);

                if (isInt == true)
                {
                    DataTable dt = new DataTable();

                    //string connString = _configuration.GetConnectionString("RestaurantDBConnection");
                    Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();

                    string sqlQuery = @"select P.ProductId,P.ProductName,PC.ProductCategoryName,UT.UnitTypeName, P.CreatedByName as CreatedBy,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, P.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,P.UpdatedByName as UpdatedBy,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, P.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from Product as P
                                    left join UnitType as UT on P.UnitTypeId=UT.UnitTypeId
                                    left join ProductCategory as PC on P.ProductCategoryId=PC.ProductCategoryId
                                    where P.ProductId=" + SearchText + " OR P.ProductName like '%" + SearchText + "%' OR PC.ProductCategoryName like '%" + SearchText + "%' OR UT.UnitTypeName like '%" + SearchText + "%' AND P.RestaurantCode='" + restaurantCode + "' order by P.ProductId DESC";

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
                                ProductModel productModel = new ProductModel();

                                productModel.ProductId = Convert.ToInt32(row["ProductId"].ToString());
                                productModel.ProductName = row["ProductName"].ToString();
                                productModel.UnitType = row["UnitTypeName"].ToString();
                                productModel.ProductCategory = row["ProductCategoryName"].ToString();
                                productModel.CreatedByName = row["CreatedBy"].ToString();
                                productModel.CreatedDate = row["CreatedDate"].ToString();
                                productModel.UpdatedByName = row["UpdatedBy"].ToString();
                                productModel.CreatedDate = row["UpdatedDate"].ToString();

                                productList.Add(productModel);
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
                }
                else
                {
                    DataTable dt = new DataTable();

                    //string connString = _configuration.GetConnectionString("RestaurantDBConnection");
                    Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();

                    string sqlQuery = @"select P.ProductId,P.ProductName,PC.ProductCategoryName,UT.UnitTypeName, P.CreatedByName as CreatedBy,P.CreatedDate,P.UpdatedByName as UpdatedBy,P.UpdatedDate
                                    from Product as P
                                    left join UnitType as UT on P.UnitTypeId=UT.UnitTypeId
                                    left join ProductCategory as PC on P.ProductCategoryId=PC.ProductCategoryId
                                    where P.ProductName like '%" + SearchText + "%' OR PC.ProductCategoryName like '%" + SearchText + "%' OR UT.UnitTypeName like '%" + SearchText + "%' AND P.RestaurantCode='" + restaurantCode + "' order by P.ProductId DESC";

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
                                ProductModel productModel = new ProductModel();

                                productModel.ProductId = Convert.ToInt32(row["ProductId"].ToString());
                                productModel.ProductName = row["ProductName"].ToString();
                                productModel.UnitType = row["UnitTypeName"].ToString();
                                productModel.ProductCategory = row["ProductCategoryName"].ToString();
                                productModel.CreatedByName = row["CreatedBy"].ToString();
                                productModel.CreatedDate = row["CreatedDate"].ToString();
                                productModel.UpdatedByName = row["UpdatedBy"].ToString();
                                productModel.CreatedDate = row["UpdatedDate"].ToString();

                                productList.Add(productModel);
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
                }
                return productList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<ProductModel> GetProductByID(int id, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetProductByID";

            try
            {
                ProductModel productModel = new ProductModel();

                var productdata = await _DBContext.Product.Where(x => x.ProductId == id && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();
                var unitTypeData = _DBContext.UnitType.Where(x => x.UnitTypeId == productdata.UnitTypeId && x.RestaurantCode == restaurantCode).FirstOrDefault();
                var productCategoryData = _DBContext.ProductCategory.Where(x => x.ProductCategoryId == productdata.ProductCategoryId && x.RestaurantCode == restaurantCode).FirstOrDefault();

                productModel.ProductId = productdata.ProductId;
                productModel.ProductName = productdata.ProductName;
                productModel.UnitTypeId = unitTypeData.UnitTypeId;
                productModel.UnitTypeName = unitTypeData.UnitTypeName;
                productModel.ProductCategoryId = productCategoryData.ProductCategoryId;
                productModel.ProductCategoryName = productCategoryData.ProductCategoryName;



                return productModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public Product EditProduct(Product product, int loggedInUserId, string restaurantCode)
        {
            string actionName = "EditProduct";
            try
            {
                var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();
                var existProduct = _DBContext.Product.Where(x => x.ProductId == product.ProductId && x.RestaurantCode == restaurantCode).FirstOrDefault();

                existProduct.ProductName = product.ProductName;
                existProduct.UnitTypeId = product.UnitTypeId;
                existProduct.ProductCategoryId = product.ProductCategoryId;
                existProduct.UpdatedByName = userData.UserName;
                existProduct.UpdatedBy = loggedInUserId;
                existProduct.UpdatedDate = DateTime.UtcNow;

                _DBContext.SaveChanges();

                return existProduct;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<string> RemoveProduct(int productId,string code, int loggedInUserId, string restaurantCode)
        {
            string actionName = "RemoveProduct";

            try
            {
                var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();
                var verificationData = await _DBContext.VarificationCode.Where(x => x.Username == userData.UserName && x.IsDeleted == false && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                if (verificationData.Code == code)
                {
                    var productData = await _DBContext.Product.Where(x => x.ProductId == productId && x.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                    _DBContext.Product.Remove(productData);
                    _DBContext.SaveChanges();

                    return "true";
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

        public async Task<List<UnitType>> UnitTypeList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "UnitTypeList";
            try
            {
                List<UnitType> unitTypeData = await _DBContext.UnitType.Where(x => x.RestaurantCode == restaurantCode).ToListAsync();

                return unitTypeData;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<ProductCategory>> ProductCategoryList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "ProductCategoryList";
            try
            {
                List<ProductCategory> productCategoryData = await _DBContext.ProductCategory.Where(x => x.RestaurantCode == restaurantCode).ToListAsync();

                return productCategoryData;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }
    }
}
