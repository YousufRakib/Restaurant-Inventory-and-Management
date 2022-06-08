using _3DPOSRegistrationApp.Database.Model;
using _3DPOSRegistrationApp.Utility.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.IRepository.ICommonRepository
{
    public interface ICommonRepository
    {
        Task<List<DistrictModel>> GetDistrictListBySearchValue(string SearchText, int loggedInUserId);
        Task<List<DistrictModel>> GetDistrictList(int loggedInUserId);
        District SaveDistrict(DistrictModel district, int loggedInUserId);
        Task<List<Country>> GetCountryListBySearchValue(string SearchText, int loggedInUserId);
        Task<List<Country>> GetCountryList(int loggedInUserId);
        Country SaveCountry(CountryModel country, int loggedInUserId);
        string CheckExistCountry(string countryName, int loggedInUserId);
        string CheckExistDistrict(string districtName, int loggedInUserId);
    }
}
