using _3DPOSRegistrationApp.Database;
using _3DPOSRegistrationApp.Database.Model;
using _3DPOSRegistrationApp.Repository.IRepository.ICommonRepository;
using _3DPOSRegistrationApp.Repository.IRepository.ILogRepository;
using _3DPOSRegistrationApp.Utility.CommonModel;
using _3DPOSRegistrationApp.Utility.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.Repository.CommonRepository
{
    public class CommonRepository: ICommonRepository
    {
        private const string _areaName = "3DPOSRegistration Module";
        private const string _controllerName = "CommonRepository";
        private readonly _3DPOS_DBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly ReturnTypeModel returnTypeModel = new ReturnTypeModel();

        public CommonRepository(_3DPOS_DBContext DBContext, IErrorLogRepository errorLogRepository)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
        }
        public Country SaveCountry(CountryModel country, int loggedInUserId)
        {
            string actionName = "SaveCountry";
            try
            {
                Country countryData = new Country();

                countryData.CountryName = country.CountryName;

                _DBContext.Country.Add(countryData);
                _DBContext.SaveChanges();

                return countryData;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<Country>> GetCountryList(int loggedInUserId)
        {
            string actionName = "GetCountryList";
            try
            {
                List<Country> countryList = new List<Country>();

                var countryListData = _DBContext.Country.OrderByDescending(x => x.CountryID).ToList();

                foreach (var data in countryListData)
                {
                    countryList.Add(new Country
                    {
                        CountryID = data.CountryID,
                        CountryName = data.CountryName
                    });
                }

                return countryList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<Country>> GetCountryListBySearchValue(string SearchText, int loggedInUserId)
        {
            string actionName = "GetCountryListBySearchValue";
            try
            {
                List<Country> countryList = new List<Country>();

                var countryListData = _DBContext.Country
                    .Where(x => x.CountryName.Contains(SearchText.ToString()))
                    .OrderByDescending(x => x.CountryID).ToList();

                foreach (var data in countryListData)
                {
                    countryList.Add(new Country
                    {
                        CountryID = data.CountryID,
                        CountryName = data.CountryName
                    });
                }
                return countryList;

            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public District SaveDistrict(DistrictModel districtModel, int loggedInUserId)
        {
            string actionName = "SaveDistrict";
            try
            {
                District district = new District();
                district.DistrictName = districtModel.DistrictName;
                district.CountryID = districtModel.CountryID;

                _DBContext.District.Add(district);
                _DBContext.SaveChanges();

                return district;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<DistrictModel>> GetDistrictList(int loggedInUserId)
        {
            string actionName = "GetDistrictList";
            try
            {
                List<DistrictModel> districtList = new List<DistrictModel>();

                var districtListData = _DBContext.District
                    .Join(_DBContext.Country,D=>D.CountryID,C=>C.CountryID,(D,C)=>new { D,C})
                    .Select(x=>new
                    {
                        x.D.DistrictID,
                        x.D.DistrictName,
                        x.C.CountryID,
                        x.C.CountryName
                    }).OrderByDescending(x => x.DistrictID).ToList();

                foreach (var data in districtListData)
                {
                    districtList.Add(new DistrictModel
                    {
                        DistrictID= data.DistrictID,
                        DistrictName= data.DistrictName,
                        CountryID= data.CountryID,
                        CountryName= data.CountryName
                    });
                }

                return districtList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<DistrictModel>> GetDistrictListBySearchValue(string SearchText, int loggedInUserId)
        {
            string actionName = "GetCountryListBySearchValue";
            try
            {
                List<DistrictModel> districtList = new List<DistrictModel>();

                var districtListData = _DBContext.District
                    .Join(_DBContext.Country, D => D.CountryID, C => C.CountryID, (D, C) => new { D, C })
                    .Where(x => x.C.CountryName.Contains(SearchText.ToString()) || x.D.DistrictName.Contains(SearchText.ToString()))
                    .Select(x => new
                    {
                        x.D.DistrictID,
                        x.D.DistrictName,
                        x.C.CountryID,
                        x.C.CountryName
                    }).OrderByDescending(x => x.DistrictID).ToList();

                foreach (var data in districtListData)
                {
                    districtList.Add(new DistrictModel
                    {
                        DistrictID = data.DistrictID,
                        DistrictName = data.DistrictName,
                        CountryID = data.CountryID,
                        CountryName = data.CountryName
                    });
                }

                return districtList;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public string CheckExistCountry(string countryName, int loggedInUserId)
        {
            string actionName = "CheckExistCountry";
            try
            {
                var isExist = _DBContext.Country.Where(x => x.CountryName == countryName).FirstOrDefault();

                if (isExist != null)
                {
                    return returnTypeModel.False;
                }
                else
                {
                    return returnTypeModel.True;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return returnTypeModel.Exception;
            }
        }

        public string CheckExistDistrict(string districtName, int loggedInUserId)
        {
            string actionName = "CheckExistDistrict";
            try
            {
                var isExist = _DBContext.District.Where(x => x.DistrictName == districtName).FirstOrDefault();

                if (isExist != null)
                {
                    return returnTypeModel.False;
                }
                else
                {
                    return returnTypeModel.True;
                }
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return returnTypeModel.Exception;
            }
        }
    }
}
