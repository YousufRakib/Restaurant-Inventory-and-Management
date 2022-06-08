using CommonEntityModel.EntityModel;
using _3DPOSRegistrationApp.Utility.LogViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.IRepository.ILogRepository
{
    public interface IAuditLogRepository
    {
        bool SaveAuditLog(AuditLog auditLog, int loggedInUserId);
        Task<List<AuditLogViewModel>> GetAuditLogList(int loggedInUserId);
        Task<List<AuditLogViewModel>> GetAuditLogListBySearchValue(string AuditByTxt,string ControllerNameTxt, string ActionNameTxt, string PreInfoTxt, string UpdatedInfoNameTxt, int loggedInUserId);
    }
}
