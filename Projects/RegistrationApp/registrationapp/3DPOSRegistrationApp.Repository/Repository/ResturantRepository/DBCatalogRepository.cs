using _3DPOSRegistrationApp.Database;
using _3DPOSRegistrationApp.Database.Model;
using _3DPOSRegistrationApp.Repository.IRepository.ILogRepository;
using _3DPOSRegistrationApp.Repository.IRepository.IResturantRepository;
using _3DPOSRegistrationApp.Utility.CommonModel;
using _3DPOSRegistrationApp.Utility.ResturantModel;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.Repository.ResturantRepository
{
    public class DBCatalogRepository : IDBCatalogRepository
    {
        private const string _areaName = "3DPOSRegistration Module";
        private const string _controllerName = "DBCatalogRepository";
        private readonly _3DPOS_DBContext _DBContext;
        private readonly RestaurantDbContext _restaurantDbContext;
        private readonly IErrorLogRepository _errorLogRepository;

        public DBCatalogRepository(_3DPOS_DBContext DBContext, RestaurantDbContext restaurantDbContext, IErrorLogRepository errorLogRepository)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
            this._restaurantDbContext = restaurantDbContext;
        }

        public DBCatalog SaveDBCatalog(DBCatalogViewModel _DBCatalogViewModel, int loggedInUserId)
        {
            string actionName = "SaveDBCatalog";
            try
            {
                var existDBCatalog = _DBContext.DBCatalog.Where(x => x.DatabaseName == _DBCatalogViewModel.DatabaseName || x.ConnectionString == _DBCatalogViewModel.ConnectionString).FirstOrDefault();

                if (existDBCatalog == null)
                {
                    DBCatalog dBCatalog = new DBCatalog();

                    dBCatalog.DatabaseName = _DBCatalogViewModel.DatabaseName;
                    dBCatalog.ConnectionString = _DBCatalogViewModel.ConnectionString;
                    dBCatalog.SiteUrl = _DBCatalogViewModel.SiteUrl;
                    dBCatalog.CanInsertRestaurant = _DBCatalogViewModel.CanInsertRestaurant == "True" ? true : false;
                    dBCatalog.IsDeleted = false;
                    dBCatalog.CreatedBy = loggedInUserId;
                    dBCatalog.CreatedDate = DateTime.UtcNow;
                    dBCatalog.IsDBCreated = false;
                    dBCatalog.RestaurantCount = 0;

                    _DBContext.DBCatalog.Add(dBCatalog);
                    _DBContext.SaveChanges();

                    return dBCatalog;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<DBCatalogViewModel>> GetDBCatalogList(int loggedInUserId)
        {
            string actionName = "GetDBCatalogList";
            try
            {
                List<DBCatalogViewModel> DBCatalogViewModelList = new List<DBCatalogViewModel>();

                var resturantData = await
                    (from R in _DBContext.DBCatalog
                     from U in _DBContext.Users.Where(U => U.Id == R.CreatedBy).DefaultIfEmpty()

                     select new
                     {
                         R.ID,
                         R.DatabaseName,
                         R.ConnectionString,
                         R.SiteUrl,
                         R.IsDeleted,
                         R.CanInsertRestaurant,
                         R.CreatedDate,
                         R.RestaurantCount,
                         R.IsDBCreated,
                         R.MigrationFile,
                         CreatedBy = U.UserName == null ? "No User Logged In" : U.UserName
                     })
                     .Where(x => x.IsDeleted==false)
                     .OrderByDescending(x => x.ID).ToListAsync();

                foreach (var data in resturantData)
                {
                    DBCatalogViewModelList.Add(new DBCatalogViewModel
                    {
                        ID = data.ID,
                        DatabaseName = data.DatabaseName,
                        ConnectionString = data.ConnectionString,
                        CanInsertRestaurant = data.CanInsertRestaurant.ToString(),
                        RestaurantCount=data.RestaurantCount,
                        CreatedDate =Convert.ToDateTime(data.CreatedDate),
                        CreatedBy = data.CreatedBy,
                        SiteUrl = data.SiteUrl,
                        MigrationFile=data.MigrationFile,
                        IsDatabase_TableCreated= data.IsDBCreated
                    });
                }

                return DBCatalogViewModelList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<DBCatalogViewModel>> GetDBCatalogListBySearchValue(string SearchText, int loggedInUserId)
        {
            string actionName = "GetDBCatalogListBySearchValue";
            try
            {
                List<DBCatalogViewModel> DBCatalogViewModelList = new List<DBCatalogViewModel>();

                var resturantData = await
                    (from R in _DBContext.DBCatalog
                     from U in _DBContext.Users.Where(U => U.Id == R.CreatedBy).DefaultIfEmpty()

                     select new
                     {
                         R.ID,
                         R.DatabaseName,
                         R.ConnectionString,
                         R.SiteUrl,
                         R.IsDeleted,
                         R.CanInsertRestaurant,
                         R.RestaurantCount,
                         R.CreatedDate,
                         R.IsDBCreated,
                         R.MigrationFile,
                         CreatedBy = U.UserName == null ? "No User Logged In" : U.UserName
                     })
                     .Where(x => x.IsDeleted == false && x.DatabaseName.Contains(SearchText.ToString()) || x.ConnectionString.Contains(SearchText.ToString()) || x.CreatedBy.Contains(SearchText.ToString()) || x.DatabaseName.Contains(SearchText.ToString()))
                     .OrderByDescending(x => x.ID).ToListAsync();

                foreach (var data in resturantData)
                {
                    DBCatalogViewModelList.Add(new DBCatalogViewModel
                    {
                        ID = data.ID,
                        DatabaseName = data.DatabaseName,
                        ConnectionString = data.ConnectionString,
                        CanInsertRestaurant = data.CanInsertRestaurant.ToString(),
                        RestaurantCount = data.RestaurantCount,
                        CreatedDate =Convert.ToDateTime(data.CreatedDate),
                        CreatedBy = data.CreatedBy,
                        SiteUrl = data.SiteUrl,
                        MigrationFile = data.MigrationFile,
                        IsDatabase_TableCreated = data.IsDBCreated
                    });
                }

                return DBCatalogViewModelList;

            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<DBCatalogViewModel> GetDBCatalogByID(int id, int loggedInUserId)
        {
            string actionName = "GetDBCatalogByID";
            DBCatalogViewModel dbCatalogViewModel = new DBCatalogViewModel();

            try
            {
                var dbCatalog =await _DBContext.DBCatalog.Where(x => x.ID == id).FirstOrDefaultAsync();

                dbCatalogViewModel.ID = dbCatalog.ID;
                dbCatalogViewModel.DatabaseName = dbCatalog.DatabaseName;
                dbCatalogViewModel.ConnectionString = dbCatalog.ConnectionString;
                dbCatalogViewModel.SiteUrl = dbCatalog.SiteUrl;
                dbCatalogViewModel.CanInsertRestaurant = dbCatalog.CanInsertRestaurant.ToString();
                dbCatalogViewModel.IsTryCatchError = false;

                return dbCatalogViewModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                dbCatalogViewModel.IsTryCatchError = true;
                return dbCatalogViewModel;
            }
        }

        public async Task<string> RemoveDBCatalog(int id, int loggedInUserId)
        {
            string actionName = "RemoveDBCatalog";

            try
            {
                var dbCatalog = await _DBContext.DBCatalog.Where(x => x.ID == id).FirstOrDefaultAsync();

                var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>().UseSqlServer(dbCatalog.ConnectionString);
                using (var context = new RestaurantDbContext(optionsBuilder.Options))
                {
                    var restauranrCount = context.Resturant.Count();

                    if(restauranrCount<0 || restauranrCount == null)
                    {
                        dbCatalog.IsDeleted = true;
                        _DBContext.SaveChanges();

                        return "true";
                    }
                    else
                    {
                        return "false";
                    }
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return "tryCatch";
            }
        }

        public DBCatalog EditDBCatalog(DBCatalogViewModel _DBCatalogViewModel, int loggedInUserId)
        {
            string actionName = "EditDBCatalog";
            try
            {
                var existDBCatalog = _DBContext.DBCatalog.Where(x => x.ID == _DBCatalogViewModel.ID).FirstOrDefault();
                
                DBCatalog dBCatalog = new DBCatalog();

                existDBCatalog.DatabaseName = _DBCatalogViewModel.DatabaseName;
                existDBCatalog.SiteUrl = _DBCatalogViewModel.SiteUrl;
                existDBCatalog.CanInsertRestaurant = _DBCatalogViewModel.CanInsertRestaurant == "True" ? true : false;
                existDBCatalog.UpdatedBy = loggedInUserId;
                existDBCatalog.UpdatedDate = DateTime.UtcNow;

                _DBContext.SaveChanges();

                return existDBCatalog;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<bool> AddLastMigrationFileInDBCatalog(string fileName,int id, int loggedInUserId)
        {
            string actionName = "AddLastMigrationFileInDBCatalog";
            try
            {
                var existDBCatalog =await _DBContext.DBCatalog.Where(x => x.ID == id).FirstOrDefaultAsync();

                existDBCatalog.MigrationFile = fileName;

                _DBContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return false;
            }
        }

        public bool AddDatabaseAndTable(int id, int loggedInUserId)
        {
            string actionName = "AddDatabaseAndTable";
            try
            {
                var existDBCatalog = _DBContext.DBCatalog.Where(x => x.ID == id).FirstOrDefault();

                DBCatalog dBCatalog = new DBCatalog();
                existDBCatalog.IsDBCreated = true;

                _DBContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return false;
            }
        }
    }
}
