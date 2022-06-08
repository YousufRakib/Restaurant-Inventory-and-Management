using _3DPOSRegistrationApp.Database.Model;
using _3DPOSRegistrationApp.Utility.UserRoleModel;
using CommonEntityModel.EntityModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.IRepository.ILogRepository
{
    public interface IRoleLogRepository
    {
        Task<List<UserRoleUpdateLogModel>> GetUserRoleLogList(int loggedInUserId);
        Task<List<UserRoleUpdateLogModel>> GetUserRoleLogDataBySearchValue(string SearchText, int loggedInUserId);
        bool SaveRoleUpdateLog(RoleUpdateHistory roleUpdateHistory, int loggedInUserId);
    }
}
