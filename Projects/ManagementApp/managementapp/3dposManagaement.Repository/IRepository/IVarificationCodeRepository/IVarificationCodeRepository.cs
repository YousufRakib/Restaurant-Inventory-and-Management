using _3dposManagaement.Utility.MenuModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.IRepository.IVarificationCodeRepository
{
    public interface IVarificationCodeRepository
    {
        Task<List<VarificationCodeUserModel>> NotExistUserInVarificationCode(int loggedInUserId, string restaurantCode);
        Task<VarificationCodeViewModel> SaveVarificationCode(VarificationCodeViewModel varificationCodeData, int loggedInUserId, string restaurantCode);
        Task<List<VarificationCodeViewModel>> GetVarificationCodeList(int loggedInUserId, string restaurantCode);
        Task<List<VarificationCodeViewModel>> GetVarificationCodeListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode);
        Task<VarificationCodeViewModel> GetVarificationCodeById(int loggedInUserId, string restaurantCode, int varificationCodeId);
        Task<string> DeleteVarificationCodeById(int loggedInUserId, string restaurantCode, int varificationCodeId, string code);
        Task<VarificationCodeViewModel> UpdateVarificationCode(VarificationCodeViewModel varificationCodeData, int loggedInUserId, string restaurantCode);
    }
}
