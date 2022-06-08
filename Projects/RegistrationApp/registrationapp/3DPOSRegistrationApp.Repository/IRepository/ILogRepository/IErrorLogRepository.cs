using _3DPOSRegistrationApp.Utility.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.IRepository.ILogRepository
{
    public interface IErrorLogRepository
    {
        bool InsertErrorToDatabase(string areaName, string controllerName, string actionName, string errorMessage, int errorFromUser);
        Task<List<CommonInfo>> GetErrorLogList(int loggedInUserId);
        Task<List<CommonInfo>> GetErrorLogListBySearchValue(string SearchText, int loggedInUserId);
        Task<CommonInfo> GetErrorDetails(int errorLogId, int loggedInUserId);
    }
}
