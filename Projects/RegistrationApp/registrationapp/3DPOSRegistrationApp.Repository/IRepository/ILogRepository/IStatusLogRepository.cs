using _3DPOSRegistrationApp.Database.Model;
using _3DPOSRegistrationApp.Utility.UserStatus;
using CommonEntityModel.EntityModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.IRepository.ILogRepository
{
    public interface IStatusLogRepository
    {
        Task<List<UserStatusUpdateLogModel>> GetUserStatusLogList(int loggedInUserId);
        Task<List<UserStatusUpdateLogModel>> GetUserStatusLogDataBySearchValue(string SearchText, int loggedInUserId);
        bool SaveUserStatusUpdateLog(UserStatusUpdateLog roleUpdateHistory, int loggedInUserId);
    }
}
