using CommonEntityModel.EntityModel;
using _3DPOSRegistrationApp.Utility.CommonModel;
using _3DPOSRegistrationApp.Utility.ResturantModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.IRepository.IResturantRepository
{
    public interface IResturantRepository
    {
        Resturant SaveResturant(ResturantModel resturant, int loggedInUserId);
        Task<List<ResturantModel>> GetResturantList(int loggedInUserId);
        Task<List<ResturantModel>> GetResturantListBySearchValue(string SearchText, int loggedInUserId);
        Task<ResturantModel> GetResturantInfoByID(int id, int loggedInUserId);
        bool AddDatabaseToRestaurant(ResturantModel resturant, int loggedInUserId);
        Task<List<RestaurantUsers>> RestaurantUserList(int loggedInUserId=0, int id=0);
        Task<List<RestaurantUsers>> RestaurantUserListBySearchValue(string SearchText = "", int loggedInUserId = 0, int id = 0);
        //bool CreateRestaurantUser(string restaurantCode, int loggedInUserId,string dbConn);
        Task<Resturant> CreateRestaurantAndRestaurantUserInDBCatalogDatabase(ResturantModel resturantModel, int loggedInUserId);
        Task<Resturant> UpdateRestaurantInformation(ResturantModel resturantModel, int loggedInUserId);
        Task<bool> RemoveRestaurant(int id, int loggedInUserId);
        Task<List<RestaurantUserRole>> GetRestaurantUserRoles(int loggedInUserId = 0, int id = 0);
        Task<string> CheckExistRestaurantUser(RestaurantUserWithDatabase restaurantUserWithDatabase, int loggedInUserId = 0);
        Task<RestaurantUserWithDatabase> SaveNewRestaurantUser(RestaurantUserWithDatabase restaurantUserWithDatabase, int loggedInUserId = 0);
    }
}
