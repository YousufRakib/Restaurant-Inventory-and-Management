using _3DPOSRegistrationApp.Database;
using _3DPOSRegistrationApp.Database.Model;
using _3DPOSRegistrationApp.Repository.IRepository.ILogRepository;
using _3DPOSRegistrationApp.Utility.UserRoleModel;
using CommonEntityModel.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.Repository.LogRepository
{
    public class RoleLogRepository: IRoleLogRepository
    {
        private const string _areaName = "3DPOSRegistration Module";
        private const string _controllerName = "RoleLogRepository";
        private readonly _3DPOS_DBContext _DBContext;
        private readonly IErrorLogRepository _errorLogRepository;

        public RoleLogRepository(_3DPOS_DBContext DBContext,IErrorLogRepository errorLogRepository)
        {
            this._DBContext = DBContext;
            this._errorLogRepository = errorLogRepository;
        }

        public async Task<List<UserRoleUpdateLogModel>> GetUserRoleLogList(int loggedInUserId)
        {
            string actionName = "GetUserRoleLogList";
            try
            {
                List<UserRoleUpdateLogModel> userRoleUpdateLogModel = new List<UserRoleUpdateLogModel>();
                var userRoleUpdateLogData = _DBContext.RoleUpdateHistory
                     .Join(_DBContext.Users, UH => UH.UpdatedBy, U => U.Id, (UH, U) => new { UH, U })
                     .Select(x => new
                     {
                         Id = x.UH.Id,
                         UserId = x.UH.UserId,
                         UserName = x.UH.Username,
                         PreviousRole = x.UH.PreviousRole,
                         UpdatedRole = x.UH.UpdatedRole,
                         UpdatedBy = x.U.UserName,
                         UpdatedDate = x.UH.UpdatedDate
                     }).OrderByDescending(x => x.Id).ToList();

                foreach (var data in userRoleUpdateLogData)
                {
                    userRoleUpdateLogModel.Add(new UserRoleUpdateLogModel
                    {
                        Id = data.Id,
                        UserId = data.UserId,
                        Username = data.UserName,
                        PreviousRole = data.PreviousRole,
                        UpdatedRole = data.UpdatedRole,
                        UpdatedBy = data.UpdatedBy,
                        UpdatedDate = data.UpdatedDate
                    });
                }
                return userRoleUpdateLogModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<UserRoleUpdateLogModel>> GetUserRoleLogDataBySearchValue(string SearchText, int loggedInUserId)
        {
            string actionName = "GetUserRoleLogDataBySearchValue";
            try
            {
                List<UserRoleUpdateLogModel> userRoleUpdateLogModel = new List<UserRoleUpdateLogModel>();
                var userRoleUpdateLogData = _DBContext.RoleUpdateHistory
                     .Join(_DBContext.Users, UH => UH.UpdatedBy, U => U.Id, (UH, U) => new { UH, U })
                     .Where(x => x.UH.Username.Contains(SearchText.ToString()) || x.UH.UpdatedRole.Contains(SearchText.ToString()) || x.UH.PreviousRole.Contains(SearchText.ToString()) || x.U.UserName.Contains(SearchText.ToString()))
                     .Select(x => new
                     {
                         Id = x.UH.Id,
                         UserId = x.UH.UserId,
                         UserName = x.UH.Username,
                         PreviousRole = x.UH.PreviousRole,
                         UpdatedRole = x.UH.UpdatedRole,
                         UpdatedBy = x.U.UpdatedBy,
                         UpdatedDate = x.UH.UpdatedDate
                     }).OrderByDescending(x => x.Id).ToList();

                foreach (var data in userRoleUpdateLogData)
                {
                    userRoleUpdateLogModel.Add(new UserRoleUpdateLogModel
                    {
                        Id = data.Id,
                        UserId = data.UserId,
                        Username = data.UserName,
                        PreviousRole = data.PreviousRole,
                        UpdatedRole = data.UpdatedRole,
                        UpdatedBy = data.UserName,
                        UpdatedDate = data.UpdatedDate
                    });
                }
                return userRoleUpdateLogModel;
            }
            catch (Exception ex)
            {
                bool error = _errorLogRepository.InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public bool SaveRoleUpdateLog(RoleUpdateHistory roleUpdateHistory, int loggedInUserId)
        {
            string actionName = "SaveRoleUpdateLog";

            try
            {
                _DBContext.RoleUpdateHistory.Add(roleUpdateHistory);
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
