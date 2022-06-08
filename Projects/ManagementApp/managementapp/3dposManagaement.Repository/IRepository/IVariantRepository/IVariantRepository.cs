using _3dposManagaement.Utility.MenuModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.IRepository.IVariantRepository
{
    public interface IVariantRepository
    {
        Task<VariantMargeViewModel> SaveVariant(VariantMargeViewModel variantMargeViewModel, int loggedInUserId, string restaurantCode);
        Task<VariantMargeViewModel> GetVariantList(int loggedInUserId, string restaurantCode);
        Task<VariantMargeViewModel> GetVariantListBySearchValue(string SearchText, int loggedInUserId, string restaurantCode);
        Task<string> ChangeVariantStatusById(int loggedInUserId, string restaurantCode, int variantId);
    }
}
