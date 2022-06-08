using _3dposManagaement.Database;
using _3dposManagaement.Repository.IRepository.IInventoryRepository;
using _3dposManagaement.Repository.IRepository.ILogRepository;
using _3dposManagaement.Repository.IRepository.IProductRepository;
using _3dposManagaement.Utility.CommonModel;
using CommonEntityModel.EntityModel;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.Repository.InventoryRepository
{
    public class InventoryRepository : IInventoryRepository
    {
        private const string _areaName = "3DPOSManagement Module";
        private const string _controllerName = "InventoryRepository";
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IConfiguration _configuration;

        public InventoryRepository(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository, IConfiguration configuration)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._configuration = configuration;
        }

        //public InventoryModel SaveInventory(InventoryModel inventory, int loggedInUserId, string restaurantCode)
        //{
        //    string actionName = "SaveInventory";
        //    try
        //    {
        //        using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                //var userData = _DBContext.Users.Where(x => x.Id == loggedInUserId).FirstOrDefault();

        //                InventoryModel inventoryModelData = new InventoryModel();

        //                inventoryModelData.ProductId = inventory.ProductId;
        //                inventoryModelData.Price = inventory.Price;
        //                inventoryModelData.Quantity = inventory.Quantity;
        //                inventoryModelData.Total = inventory.Total;
        //                inventoryModelData.Comment = inventory.Comment;
        //                inventoryModelData.CreatedBy = loggedInUserId;
        //                inventoryModelData.CreatedDate = DateTime.UtcNow;
        //                inventoryModelData.InventoryTransactionTypeId = 1;

        //                _DBContext.InventoryModel.Add(inventoryModelData);
        //                _DBContext.SaveChanges();

        //                var productData = _DBContext.Product.Where(x => x.ProductId == inventory.ProductId && x.RestaurantCode == restaurantCode).OrderByDescending(x=>x.ProductId).Take(1).FirstOrDefault();

        //                productData.Price = productData.Price+inventory.Price;
        //                productData.Quantity = productData.Quantity+ inventory.Quantity;
        //                productData.Total = productData.Total+inventory.Total;
        //                productData.Comment = inventory.Comment;
        //                productData.UpdatedBy = loggedInUserId;
        //                productData.UpdatedDate = DateTime.UtcNow;
        //                if (HasUnsavedChanges() == true)
        //                {
        //                    var changedProductData = _DBContext.Product.Where(x => x.ProductId == inventory.ProductId && x.RestaurantCode == restaurantCode).OrderByDescending(x => x.ProductId).Take(1).FirstOrDefault();


        //                    productData.Price = productData.Price + inventory.Price;
        //                    productData.Quantity = productData.Quantity + inventory.Quantity;
        //                    productData.Total = productData.Total + inventory.Total;
        //                    productData.Comment = inventory.Comment;
        //                    productData.UpdatedBy = loggedInUserId;
        //                    productData.UpdatedDate = DateTime.UtcNow;
        //                    _DBContext.SaveChanges();
        //                }
        //                else
        //                {
        //                    _DBContext.SaveChanges();
        //                }

        //                transaction.Commit();
        //                return inventoryModelData;
        //            }
        //            catch (Exception ex)
        //            {
        //                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //                transaction.Rollback();
        //                return null;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //        return null;
        //    }
        //}

        public bool HasUnsavedChanges()
        {
            return _DBContext.ChangeTracker.Entries().Any(e => e.State == EntityState.Added
                                                      || e.State == EntityState.Modified
                                                      || e.State == EntityState.Deleted);
        }

        public async Task<List<InventoryViewModel>> GetInventoryList(int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetInventoryList";
            try
            {
                List<InventoryViewModel> inventorytList = new List<InventoryViewModel>();
                DataTable dt = new DataTable();

                //string connString = _configuration.GetConnectionString("RestaurantDBConnection");
                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();

                string sqlQuery = @"select I.InventoryId,P.ProductName,I.Price,I.Quantity,I.Total, I.CreatedByName as CreatedBy,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, I.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,I.UpdatedByName as UpdatedBy,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, I.UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from InventoryModel as I
                                    left join Product as P on I.ProductId=P.ProductId
                                    where P.RestaurantCode='" + restaurantCode + "' order by I.InventoryId DESC";

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
                            InventoryViewModel inventoryViewModel = new InventoryViewModel();

                            inventoryViewModel.InventoryId = Convert.ToInt32(row["InventoryId"].ToString());
                            inventoryViewModel.ProductName = row["ProductName"].ToString();
                            inventoryViewModel.Price = Convert.ToDecimal(row["Price"].ToString());
                            inventoryViewModel.Quantity = Convert.ToInt32(row["Quantity"].ToString());
                            inventoryViewModel.Total = Convert.ToDecimal(row["Total"].ToString());
                            inventoryViewModel.CreatedByName = row["CreatedBy"].ToString();
                            inventoryViewModel.CreatedDate = row["CreatedDate"].ToString();
                            inventoryViewModel.UpdatedByName = row["UpdatedBy"].ToString();
                            inventoryViewModel.UpdatedDate = row["UpdatedDate"].ToString();

                            inventorytList.Add(inventoryViewModel);
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


                return inventorytList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        //public async Task<List<InventoryViewModel>> GetInventoryListBySearchValue(string txtInventoryId, string txtProductName, string txtSelectedDateFrom, string txtSelectedDateTo, int loggedInUserId, string restaurantCode)
        public async Task<List<InventoryViewModel>> GetInventoryListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetInventoryListBySearchValue";
            try
            {
                //List<InventoryViewModel> inventorytList = new List<InventoryViewModel>();
                //string inventoryIdQuery = " ";
                //string productNameQuery = " ";
                //string dateQuery= " ";
                //// bool isInt = txtInventoryId.All(char.IsDigit);

                //if (txtInventoryId != null && txtInventoryId != "")
                //{
                //    inventoryIdQuery = " AND P.ProductId=" + txtInventoryId + "";
                //}
                //if (txtProductName != null && txtProductName != "")
                //{
                //    productNameQuery = " And P.ProductName like '%" + txtProductName + "%'";
                //}

                //if (txtSelectedDateFrom != null && txtSelectedDateFrom != "" && txtSelectedDateTo != null && txtSelectedDateTo != "") 
                //{
                //    dateQuery = "And (CONVERT(date,I.CreatedDate)>=CONVERT(date,'" + txtSelectedDateFrom + "')) AND (CONVERT(date,I.CreatedDate)<=CONVERT(date,'" + txtSelectedDateTo + "'))";
                //}


                string sql = $@"select I.InventoryId,P.ProductName,I.Price,I.Quantity,I.Total, I.CreatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, I.CreatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as CreatedDate,I.UpdatedByName,CONVERT(datetime,SWITCHOFFSET(CONVERT(DATETIMEOFFSET, I.UpdatedDate),DATENAME(TZOFFSET, SYSDATETIMEOFFSET()))) as UpdatedDate
                                    from InventoryModel as I
                                    left join Product as P on I.ProductId=P.ProductId
                                    where P.RestaurantCode=@RestaurantCode AND I.InventoryId=@SearchText OR P.ProductName like '%" + SearchText + "%' order by I.InventoryId DESC";

                Resturant resturant = _DBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();
                using (var connection = new SqlConnection(resturant.DBConnectionString))
                {
                    connection.Open();

                    var query = connection.Query<InventoryViewModel>(sql, new { RestaurantCode = restaurantCode, SearchText = SearchText }).ToList();

                    return query;
                }


                //DataTable dt = new DataTable();

                //string connString = _configuration.GetConnectionString("RestaurantDBConnection");
                ////Resturant resturant = _3DPOSDBContext.Resturant.Where(x => x.ResturentCode == restaurantCode).FirstOrDefault();

                //string sqlQuery = @"select I.InventoryId,P.ProductName,I.Price,I.Quantity,I.Total,ITT.Type, U1.UserName as CreatedBy,I.CreatedDate,U2.UserName as UpdatedBy,I.UpdatedDate
                //                    from InventoryModel as I
                //                    left join AspNetUsers as U1 on I.CreatedBy=U1.Id
                //                    left join AspNetUsers as U2 on I.UpdatedBy=U2.Id
                //                    left join Product as P on I.ProductId=P.ProductId
                //                    left join InventoryTransactionType as ITT on I.InventoryTransactionTypeId=ITT.Id
                //                    where P.RestaurantCode='" + restaurantCode + "' " + inventoryIdQuery + " " + productNameQuery + " " + transactionTypeQuery + " " + dateQuery + " order by I.InventoryId DESC";

                //using (SqlConnection connection = new SqlConnection(connString))
                //{
                //    try
                //    {
                //        using (SqlCommand cmd = new SqlCommand(sqlQuery, connection))
                //        {
                //            cmd.CommandTimeout = 0;
                //            cmd.CommandType = CommandType.Text;
                //            connection.Open();
                //            SqlDataAdapter da = new SqlDataAdapter(cmd);
                //            da.Fill(dt);
                //        }

                //        foreach (DataRow row in dt.Rows)
                //        {
                //            InventoryViewModel inventoryViewModel = new InventoryViewModel();

                //            inventoryViewModel.InventoryId = Convert.ToInt32(row["InventoryId"].ToString());
                //            inventoryViewModel.ProductName = row["ProductName"].ToString();
                //            inventoryViewModel.Price = Convert.ToDecimal(row["Price"].ToString());
                //            inventoryViewModel.Quantity = Convert.ToInt32(row["Quantity"].ToString());
                //            inventoryViewModel.Total = Convert.ToDecimal(row["Total"].ToString());
                //            inventoryViewModel.CreatedByName = row["CreatedBy"].ToString();
                //            inventoryViewModel.CreatedDate = row["CreatedDate"].ToString();
                //            inventoryViewModel.UpdatedByName = row["UpdatedBy"].ToString();
                //            inventoryViewModel.UpdatedDate = row["UpdatedDate"].ToString();
                //            inventoryViewModel.InventoryTransactionTypeName = row["Type"].ToString();

                //            inventorytList.Add(inventoryViewModel);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        connection.Close();
                //        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                //        return null;
                //    }
                //    connection.Close();
                //}
                //return inventorytList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<InventoryModel> GetInventoryByID(int id, int loggedInUserId, string restaurantCode)
        {
            string actionName = "GetInventoryByID";
            InventoryViewModel vm = new InventoryViewModel();
            try
            {
                //var data= await _DBContext.InventoryModel.Include(x => x.Students).Where(x => x.InventoryId == id && x.Product.RestaurantCode == restaurantCode).ToListAsync();
                //vm.inventory = await _DBContext.InventoryModel.Where(x => x.InventoryId == id && x.Product.RestaurantCode == restaurantCode).FirstOrDefaultAsync();
                // vm.products = await _DBContext.InventoryModel.Include(x => x.Product).Where(x => x.InventoryId == id).Select(x=>x.Product).ToListAsync();

                var inventoryData = await _DBContext.InventoryModel.Include(x => x.Product).Where(x => x.InventoryId == id && x.Product.RestaurantCode == restaurantCode).FirstOrDefaultAsync();

                return inventoryData;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        //public InventoryModel EditInventory(InventoryModel inventory, int loggedInUserId, string restaurantCode)
        //{
        //    string actionName = "EditInventory";
        //    try
        //    {
        //        using (IDbContextTransaction transaction = _DBContext.Database.BeginTransaction())
        //        {
        //            try
        //            {

        //                var existInventory = _DBContext.InventoryModel.Include(x=>x.Product).Where(x => x.InventoryId == inventory.InventoryId && x.Product.RestaurantCode == restaurantCode).FirstOrDefault();

        //                decimal lastPrice = existInventory.Price;
        //                int lastQuantity = existInventory.Quantity;
        //                decimal Total = existInventory.Total;

        //                existInventory.ProductId = inventory.ProductId;
        //                existInventory.Price = inventory.Price;
        //                existInventory.Quantity = inventory.Quantity;
        //                existInventory.Total = inventory.Total;
        //                existInventory.Comment = inventory.Comment;
        //                existInventory.UpdatedBy = loggedInUserId;
        //                existInventory.UpdatedDate = DateTime.UtcNow;

        //                var productData = _DBContext.Product.Where(x => x.ProductId == inventory.ProductId && x.RestaurantCode == restaurantCode).OrderByDescending(x=>x.ProductId).Take(1).FirstOrDefault();

        //                productData.Price = (productData.Price- lastPrice) + inventory.Price;
        //                productData.Quantity = (productData.Quantity- lastQuantity) + inventory.Quantity;
        //                productData.Total = (productData.Total- Total) + inventory.Total;
        //                productData.Comment = inventory.Comment;
        //                productData.UpdatedBy = loggedInUserId;
        //                productData.UpdatedDate = DateTime.UtcNow;

        //                if (HasUnsavedChanges() == true)
        //                {
        //                    var updatedInventory = _DBContext.InventoryModel.Include(x => x.Product).Where(x => x.InventoryId == inventory.InventoryId && x.Product.RestaurantCode == restaurantCode).FirstOrDefault();

        //                    decimal updatedlastPrice = updatedInventory.Price;
        //                    int updatedlastQuantity = updatedInventory.Quantity;
        //                    decimal updatedTotal = updatedInventory.Total;

        //                    updatedInventory.ProductId = inventory.ProductId;
        //                    updatedInventory.Price = inventory.Price;
        //                    updatedInventory.Quantity = inventory.Quantity;
        //                    updatedInventory.Total = inventory.Total;
        //                    updatedInventory.Comment = inventory.Comment;
        //                    updatedInventory.UpdatedBy = loggedInUserId;
        //                    updatedInventory.UpdatedDate = DateTime.UtcNow;

        //                    var updatedproductData = _DBContext.Product.Where(x => x.ProductId == inventory.ProductId && x.RestaurantCode == restaurantCode).OrderByDescending(x => x.ProductId).Take(1).FirstOrDefault();

        //                    updatedproductData.Price = (updatedproductData.Price - updatedlastPrice) + inventory.Price;
        //                    updatedproductData.Quantity = (updatedproductData.Quantity - updatedlastQuantity) + inventory.Quantity;
        //                    updatedproductData.Total = (updatedproductData.Total - Total) + inventory.Total;
        //                    updatedproductData.Comment = inventory.Comment;
        //                    updatedproductData.UpdatedBy = loggedInUserId;
        //                    updatedproductData.UpdatedDate = DateTime.UtcNow;
        //                    _DBContext.SaveChanges();

        //                    transaction.Commit();

        //                    return updatedInventory;
        //                }
        //                else
        //                {
        //                    _DBContext.SaveChanges();

        //                    transaction.Commit();

        //                    return existInventory;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //                transaction.Rollback();
        //                return null;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //        return null;
        //    }
        //}

        //public async Task<string> RemoveInventory(int id, int loggedInUserId, string restaurantCode)
        //{
        //    string actionName = "RemoveInventory";

        //    try
        //    {
        //        var existInventory = await _DBContext.InventoryModel.Include(x=>x.Product).Where(x => x.InventoryId == id && x.Product.RestaurantCode== restaurantCode).FirstOrDefaultAsync();

        //        var productData = await _DBContext.Product.Where(x => x.ProductId == existInventory.ProductId).FirstOrDefaultAsync();

        //        if (existInventory.ProductId != null)
        //        {
        //            if (productData != null)
        //            {
        //                _DBContext.InventoryModel.Remove(existInventory);
        //                _DBContext.SaveChanges();

        //                return "true";
        //            }
        //            else
        //            {
        //                return "false";
        //            }
        //        }
        //        else
        //        {
        //            _DBContext.InventoryModel.Remove(existInventory);
        //            _DBContext.SaveChanges();

        //            return "true";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //        return "tryCatch";
        //    }
        //}
    }
}
