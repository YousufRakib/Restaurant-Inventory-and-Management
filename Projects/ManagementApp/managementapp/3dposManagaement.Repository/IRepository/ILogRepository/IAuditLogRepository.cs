using CommonEntityModel.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.IRepository.ILogRepository
{
    public interface IAuditLogRepository
    {
        bool SaveAuditLog(AuditLog auditLog, int loggedInUserId);
    }
}
