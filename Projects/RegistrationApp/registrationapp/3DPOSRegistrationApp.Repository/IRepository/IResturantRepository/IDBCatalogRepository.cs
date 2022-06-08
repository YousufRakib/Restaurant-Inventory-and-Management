using _3DPOSRegistrationApp.Database.Model;
using _3DPOSRegistrationApp.Utility.ResturantModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.IRepository.IResturantRepository
{
    public interface IDBCatalogRepository
    {
        DBCatalog SaveDBCatalog(DBCatalogViewModel _DBCatalogViewModel, int loggedInUserId);
        Task<List<DBCatalogViewModel>> GetDBCatalogList(int loggedInUserId);
        Task<List<DBCatalogViewModel>> GetDBCatalogListBySearchValue(string SearchText, int loggedInUserId);
        Task<DBCatalogViewModel> GetDBCatalogByID(int id, int loggedInUserId);
        DBCatalog EditDBCatalog(DBCatalogViewModel _DBCatalogViewModel, int loggedInUserId);
        Task<string> RemoveDBCatalog(int id, int loggedInUserId);
        bool AddDatabaseAndTable(int id, int loggedInUserId);
        Task<bool> AddLastMigrationFileInDBCatalog(string fileName, int id, int loggedInUserId);
    }
}
