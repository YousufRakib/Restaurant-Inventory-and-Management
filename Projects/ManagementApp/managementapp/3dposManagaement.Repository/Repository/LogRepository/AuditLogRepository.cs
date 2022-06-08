using _3dposManagaement.Database;
using _3dposManagaement.Repository.IRepository.ILogRepository;
using CommonEntityModel.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.Repository.LogRepository
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private const string _areaName = "3DPOSManagement Module";
        private const string _controllerName = "AuditLogRepository";
        private readonly RestaurantManagementDBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;

        public AuditLogRepository(RestaurantManagementDBContext DBContext, IErrorLogRepository errorLogRepository)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
        }

        public bool SaveAuditLog(AuditLog auditLog, int loggedInUserId)
        {
            string actionName = "SaveAuditLog";
            try
            {
                AuditLog auditLogData = new AuditLog();
                auditLogData.AreaName = auditLog.AreaName;
                auditLogData.ControllerName = auditLog.ControllerName;
                auditLogData.ActionName = auditLog.ActionName;
                auditLogData.PreviousInformation = auditLog.PreviousInformation;
                auditLogData.UpdatedInformation = auditLog.UpdatedInformation;
                auditLogData.AuditTime = auditLog.AuditTime;
                auditLogData.AuditBy = auditLog.AuditBy;


                _DBContext.AuditLog.Add(auditLogData);
                _DBContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return false;
            }
        }

        //public async Task<List<AuditLogViewModel>> GetAuditLogList(int loggedInUserId)
        //{
        //    string actionName = "GetAuditLogList";
        //    var loggedInUserID = loggedInUserId;

        //    try
        //    {
        //        List<AuditLogViewModel> auditLogViewModel = new List<AuditLogViewModel>();
        //        AuditLogViewModel auditLogInfo = new AuditLogViewModel();

        //        var auditLogData = await
        //            (from E in _DBContext.AuditLog
        //             from U in _DBContext.Users.Where(U => U.Id == E.AuditBy).DefaultIfEmpty()
        //             select new
        //             {
        //                 E.AuditLogId,
        //                 E.ControllerName,
        //                 E.ActionName,
        //                 E.PreviousInformation,
        //                 E.UpdatedInformation,
        //                 AuditBy = U.UserName == null ? "No User Logged In" : U.UserName

        //             }).OrderByDescending(x => x.AuditLogId).ToListAsync();

        //        foreach (var data in auditLogData)
        //        {
        //            auditLogViewModel.Add(new AuditLogViewModel
        //            {
        //                AuditLogId = data.AuditLogId,
        //                ControllerName = data.ControllerName,
        //                ActionName = data.ActionName,
        //                PreviousInformation = data.PreviousInformation,
        //                UpdatedInformation = data.UpdatedInformation,
        //                AuditBy = data.AuditBy.ToString()
        //            });
        //        }
        //        return auditLogViewModel;
        //    }
        //    catch (Exception ex)
        //    {
        //        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //        return null;
        //    }
        //}

        //public async Task<List<AuditLogViewModel>> GetAuditLogListBySearchValue(string AuditByTxt, string ControllerNameTxt, string ActionNameTxt, string PreInfoTxt, string UpdatedInfoNameTxt, int loggedInUserId)
        //{
        //    string actionName = "GetAuditLogListBySearchValue";
        //    var loggedInUserID = loggedInUserId;
        //    try
        //    {
        //        if (ControllerNameTxt == null) { ControllerNameTxt = ""; }
        //        if (ActionNameTxt == null) { ActionNameTxt = ""; }
        //        if (PreInfoTxt == null) { PreInfoTxt = ""; }
        //        if (UpdatedInfoNameTxt == null) { UpdatedInfoNameTxt = ""; }
        //        if (AuditByTxt == null) { AuditByTxt = ""; }
        //        List<AuditLogViewModel> auditLogViewModel = new List<AuditLogViewModel>();

        //        //var auditLogData =await _DBContext.AuditLog
        //        //    .Select(x=>new {
        //        //        x.AuditLogId,
        //        //        x.ControllerName,
        //        //        x.ActionName,
        //        //        x.PreviousInformation,
        //        //        x.UpdatedInformation,
        //        //    })
        //        //    .Where(x=> (x.ControllerName.Contains(ControllerNameTxt) || ControllerNameTxt=="") 
        //        //    && (x.ActionName.Contains(ActionNameTxt) || ActionNameTxt == "")
        //        //    && (x.PreviousInformation.Contains(PreInfoTxt) || PreInfoTxt == "")
        //        //    && (x.UpdatedInformation.Contains(UpdatedInfoNameTxt) || UpdatedInfoNameTxt == ""))
        //        //    .OrderByDescending(x => x.AuditLogId).ToListAsync();

        //        var auditLogData = await (from E in _DBContext.AuditLog
        //                                  from U in _DBContext.Users.Where(U => U.Id == E.AuditBy)
        //                                  select new
        //                                  {
        //                                      E.AuditLogId,
        //                                      E.ControllerName,
        //                                      E.ActionName,
        //                                      E.PreviousInformation,
        //                                      E.UpdatedInformation,
        //                                      AuditBy = U.UserName == null ? "No User Logged In" : U.UserName
        //                                  })
        //            .Where(x => (x.ControllerName.Contains(ControllerNameTxt))
        //            && (x.ActionName.Contains(ActionNameTxt))
        //            && (x.PreviousInformation.Contains(PreInfoTxt))
        //            && (x.UpdatedInformation.Contains(UpdatedInfoNameTxt))
        //            && (x.AuditBy.Contains(AuditByTxt)))
        //            .OrderByDescending(x => x.AuditLogId).ToListAsync();

        //        foreach (var data in auditLogData)
        //        {
        //            auditLogViewModel.Add(new AuditLogViewModel
        //            {
        //                AuditLogId = data.AuditLogId,
        //                ControllerName = data.ControllerName,
        //                ActionName = data.ActionName,
        //                PreviousInformation = data.PreviousInformation,
        //                UpdatedInformation = data.UpdatedInformation,
        //                AuditBy = data.AuditBy.ToString()
        //            });
        //        }
        //        return auditLogViewModel;
        //    }
        //    catch (Exception ex)
        //    {
        //        bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
        //        return null;
        //    }
        //}
    }
}
