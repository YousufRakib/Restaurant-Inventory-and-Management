using _3dposManagaement.Utility.CommonModel;
using CommonEntityModel.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.IRepository.IUnitTypeRepository
{
    public interface IUnitTypeRepository
    {
        UnitType SaveUnitType(UnitType unitType, int loggedInUserId, string restaurantCode);
        Task<List<UniteTypeViewModel>> GetUnitTypeList(int loggedInUserId, string restaurantCode);
        Task<List<UniteTypeViewModel>> GetUnitTypeListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode);
        Task<UnitType> GetUniteTypeByID(int id, int loggedInUserId, string restaurantCode);
        UnitType EditUnitType(UnitType unitType, int loggedInUserId, string restaurantCode);
        Task<string> RemoveUnitType(int id, string code, int loggedInUserId, string restaurantCode);
        Task<string> CheckExistUniteType(UnitType unitType, int loggedInUserId, string restaurantCode);
    }
}
