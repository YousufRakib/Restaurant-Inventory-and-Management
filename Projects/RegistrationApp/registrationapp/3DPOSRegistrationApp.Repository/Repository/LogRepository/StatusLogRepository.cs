using _3DPOSRegistrationApp.Database;
using _3DPOSRegistrationApp.Database.Model;
using _3DPOSRegistrationApp.Repository.IRepository.ILogRepository;
using _3DPOSRegistrationApp.Utility.UserStatus;
using CommonEntityModel.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.Repository.LogRepository
{
    public class StatusLogRepository: IStatusLogRepository
    {
        private const string _areaName = "3DPOSRegistration Module";
        private const string _controllerName = "StatusLogRepository";
        private readonly _3DPOS_DBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;

        public StatusLogRepository(_3DPOS_DBContext DBContext, IErrorLogRepository errorLogRepository)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
        }

        public async Task<List<UserStatusUpdateLogModel>> GetUserStatusLogList(int loggedInUserId)
        {
            string actionName = "GetUserStatusLogList";
            try
            {
                List<UserStatusUpdateLogModel> userStatusUpdateLogModel = new List<UserStatusUpdateLogModel>();
                var userStatusUpdateLogData = _DBContext.UserStatusUpdateLog
                     .Join(_DBContext.Users, USUL => USUL.UpdatedBy, U => U.Id, (USUL, U) => new { USUL, U })
                     .Join(_DBContext.UserStatus, USUL2 => USUL2.USUL.PreviousStatus, US => US.UserStatusId, (USUL2, US) => new { USUL2, US })
                     .Join(_DBContext.UserStatus, USUL3 => USUL3.USUL2.USUL.CurrentStatus, US2 => US2.UserStatusId, (USUL3, US2) => new { USUL3, US2 })
                     .Select(x => new
                     {
                         Id = x.USUL3.USUL2.USUL.Id,
                         UserId = x.USUL3.USUL2.USUL.UserId,
                         UserName = x.USUL3.USUL2.USUL.Username,
                         PreviousStatus = x.USUL3.US.StatusType,
                         CurrentStatus = x.US2.StatusType,
                         UpdatedBy = x.USUL3.USUL2.U.UserName,
                         UpdatedDate = x.USUL3.USUL2.USUL.UpdatedDate
                     }).OrderByDescending(x => x.Id).ToList();

                foreach (var data in userStatusUpdateLogData)
                {
                    userStatusUpdateLogModel.Add(new UserStatusUpdateLogModel
                    {
                        Id = data.Id,
                        UserId = data.UserId,
                        Username = data.UserName,
                        PreviousStatus = data.PreviousStatus,
                        UpdatedStatus = data.CurrentStatus,
                        UpdatedBy = data.UpdatedBy,
                        UpdatedDate = data.UpdatedDate
                    });
                }
                return userStatusUpdateLogModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<UserStatusUpdateLogModel>> GetUserStatusLogDataBySearchValue(string SearchText, int loggedInUserId)
        {
            string actionName = "GetUserStatusLogDataBySearchValue";
            try
            {
                List<UserStatusUpdateLogModel> userStatusUpdateLogModel = new List<UserStatusUpdateLogModel>();
                var userStatusUpdateLogData = _DBContext.UserStatusUpdateLog
                     .Join(_DBContext.Users, USUL => USUL.UpdatedBy, U => U.Id, (USUL, U) => new { USUL, U })
                     .Join(_DBContext.UserStatus, USUL2 => USUL2.USUL.PreviousStatus, US => US.UserStatusId, (USUL2, US) => new { USUL2, US })
                     .Join(_DBContext.UserStatus, USUL3 => USUL3.USUL2.USUL.CurrentStatus, US2 => US2.UserStatusId, (USUL3, US2) => new { USUL3, US2 })
                     .Where(x => x.USUL3.USUL2.USUL.Username.Contains(SearchText.ToString()) || x.USUL3.US.StatusType.Contains(SearchText.ToString()) || x.US2.StatusType.Contains(SearchText.ToString()) || x.USUL3.USUL2.U.UserName.Contains(SearchText.ToString()))
                     .Select(x => new
                     {
                         Id = x.USUL3.USUL2.USUL.Id,
                         UserId = x.USUL3.USUL2.USUL.UserId,
                         UserName = x.USUL3.USUL2.USUL.Username,
                         PreviousStatus = x.USUL3.US.StatusType,
                         CurrentStatus = x.US2.StatusType,
                         UpdatedBy = x.USUL3.USUL2.U.UserName,
                         UpdatedDate = x.USUL3.USUL2.USUL.UpdatedDate
                     }).OrderByDescending(x => x.Id).ToList();

                foreach (var data in userStatusUpdateLogData)
                {
                    userStatusUpdateLogModel.Add(new UserStatusUpdateLogModel
                    {
                        Id = data.Id,
                        UserId = data.UserId,
                        Username = data.UserName,
                        PreviousStatus = data.PreviousStatus,
                        UpdatedStatus = data.CurrentStatus,
                        UpdatedBy = data.UpdatedBy,
                        UpdatedDate = data.UpdatedDate
                    });
                }
                return userStatusUpdateLogModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public bool SaveUserStatusUpdateLog(UserStatusUpdateLog userStatusUpdateLog, int loggedInUserId)
        {
            string actionName = "SaveUserStatusUpdateLog";
            try
            {
                _DBContext.UserStatusUpdateLog.Add(userStatusUpdateLog);
                _DBContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return false;
            }
        }
    }
}
