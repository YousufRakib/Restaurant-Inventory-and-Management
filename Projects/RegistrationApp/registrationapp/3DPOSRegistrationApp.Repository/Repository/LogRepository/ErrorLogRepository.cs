using _3DPOSRegistrationApp.Database;
using CommonEntityModel.EntityModel;
using _3DPOSRegistrationApp.Repository.IRepository.ILogRepository;
using _3DPOSRegistrationApp.Utility.ServiceModel;
using AutoMapper.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Repository.Repository.LogRepository
{
    public class ErrorLogRepository: IErrorLogRepository
    {
        private const string _areaName = "3DPOSRegistration Module";
        private const string _controllerName = "ErrorLogRepository";
        private readonly _3DPOS_DBContext _DBContext;

        public ErrorLogRepository(_3DPOS_DBContext DBContext)/*, IConfiguration configuration)*/
        {
            _DBContext = DBContext;
        }

        public bool InsertErrorToDatabase(string areaName, string controllerName, string actionName, string errorMessage,int errorFromUser)
        {
            try
            {
                var error = new ErrorLog();
                int errorId = _DBContext.ErrorLog.Max(rq => (int?)rq.ErrorLogId) ?? 0;
                errorId++;

                var rqStr = errorId.ToString().PadLeft(6, '0');
                string errorCode = "3DPOS" + DateTime.Now.Year+"-" + rqStr;
                error.ErrorCode = errorCode;
                error.ErrorTime = DateTime.UtcNow;
                error.AreaName = areaName;
                error.ActionName = actionName;
                error.ControllerName = controllerName;
                error.ErrorMessage = errorMessage;
                _DBContext.ErrorLog.Add(error);
                _DBContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<CommonInfo>> GetErrorLogList(int loggedInUserId)
        {
            string actionName = "GetErrorLogList";
            var loggedInUserID = loggedInUserId;

            try
            {
                List<CommonInfo> errorLogViewModel = new List<CommonInfo>();
                CommonInfo commonInfo = new CommonInfo();

                //var errorLogDataP2 = _DBContext.ErrorLog.ToList();
                //var errorLogDataP1 = _DBContext.ErrorLog
                //    .Join(_DBContext.Users, E => E.ErrorFromUser, U => U.Id, (E, U) => new { E, U})
                //    .Select(x => new
                //    {
                //        x.E.ErrorLogId,
                //        x.E.ControllerName,
                //        x.E.ActionName,
                //        x.E.ErrorTime,
                //        x.E.ErrorCode,
                //        x.U.UserName
                //    }).OrderByDescending(x => x.ErrorLogId).ToList();

                var errorLogData = await
                    (from E in _DBContext.ErrorLog
                     from U in _DBContext.Users.Where(U => U.Id == E.ErrorFromUser).DefaultIfEmpty()
                     select new
                     {
                         E.ErrorLogId,
                         E.ControllerName,
                         E.ActionName,
                         E.ErrorTime,
                         E.ErrorCode,
                         UserName =U.UserName == null ? "No User Logged In" : U.UserName

                     }).OrderByDescending(x => x.ErrorLogId).ToListAsync();

                foreach (var data in errorLogData)
                {
                    errorLogViewModel.Add(new CommonInfo
                    {
                        ErrorLogId = data.ErrorLogId,
                        ControllerName = data.ControllerName,
                        ActionName = data.ActionName,
                        ErrorTime = Convert.ToDateTime(data.ErrorTime),
                        ErrorCode = data.ErrorCode,
                        ErrorFromUserName = data.UserName.ToString()
                    });
                }
                return errorLogViewModel;
            }
            catch (Exception ex)
            {
                bool error = InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<List<CommonInfo>> GetErrorLogListBySearchValue(string SearchText, int loggedInUserId)
        {
            string actionName = "GetErrorLogListBySearchValue";
            var loggedInUserID = loggedInUserId;
            try
            {
                List<CommonInfo> errorLogViewModel = new List<CommonInfo>();

                var errorLogData = _DBContext.ErrorLog
                    .Join(_DBContext.Users, E => E.ErrorFromUser, U => U.Id, (E, U) => new { E, U })
                    .Where(x => x.E.ErrorCode.Contains(SearchText.ToString()) || x.U.UserName.Contains(SearchText.ToString()) || x.E.ControllerName.Contains(SearchText.ToString()) || x.E.ActionName.Contains(SearchText.ToString()))
                    .Select(x => new
                    {
                        x.E.ErrorLogId,
                        x.E.ControllerName,
                        x.E.ActionName,
                        x.E.ErrorTime,
                        x.E.ErrorCode,
                        x.U.UserName

                    }).OrderByDescending(x => x.ErrorLogId).ToList();

                foreach (var data in errorLogData)
                {
                    errorLogViewModel.Add(new CommonInfo
                    {
                        ErrorLogId = data.ErrorLogId,
                        ControllerName = data.ControllerName,
                        ActionName = data.ActionName,
                        ErrorTime = Convert.ToDateTime(data.ErrorTime),
                        ErrorCode = data.ErrorCode,
                        ErrorFromUserName = data.UserName
                    });
                }
                return errorLogViewModel;
            }
            catch (Exception ex)
            {
                bool error = InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return null;
            }
        }

        public async Task<CommonInfo> GetErrorDetails(int errorLogId, int loggedInUserId)
        {
            CommonInfo errorLogViewModel = new CommonInfo();
            string actionName = "GetErrorDetails";
            var loggedInUserID = loggedInUserId;
            try
            {
                var errorLogData = _DBContext.ErrorLog
                    .Join(_DBContext.Users, E => E.ErrorFromUser, U => U.Id, (E, U) => new { E, U })
                    .Where(x => x.E.ErrorLogId == errorLogId)
                    .Select(x => new
                    {
                        x.E.ErrorLogId,
                        x.E.ControllerName,
                        x.E.ActionName,
                        x.E.ErrorTime,
                        x.E.ErrorCode,
                        x.E.ErrorMessage,
                        x.U.UserName

                    }).FirstOrDefault();

                if (errorLogData != null)
                {
                    errorLogViewModel.ErrorLogId = errorLogData.ErrorLogId;
                    errorLogViewModel.ControllerName = errorLogData.ControllerName;
                    errorLogViewModel.ActionName = errorLogData.ActionName;
                    errorLogViewModel.ErrorTime = Convert.ToDateTime(errorLogData.ErrorTime);
                    errorLogViewModel.ErrorCode = errorLogData.ErrorCode;
                    errorLogViewModel.ErrorFromUserName = errorLogData.UserName;
                    errorLogViewModel.ErrorMessage = errorLogData.ErrorMessage;
                    errorLogViewModel.IsTrue = true;

                    return errorLogViewModel;
                }
                else
                {
                    var notLoggedInErrorLogData= _DBContext.ErrorLog.Where(x => x.ErrorLogId == errorLogId).FirstOrDefault();

                    errorLogViewModel.ErrorLogId = notLoggedInErrorLogData.ErrorLogId;
                    errorLogViewModel.ControllerName = notLoggedInErrorLogData.ControllerName;
                    errorLogViewModel.ActionName = notLoggedInErrorLogData.ActionName;
                    errorLogViewModel.ErrorTime = Convert.ToDateTime(notLoggedInErrorLogData.ErrorTime);
                    errorLogViewModel.ErrorCode = notLoggedInErrorLogData.ErrorCode;
                    errorLogViewModel.ErrorFromUserName = "No user logged in";
                    errorLogViewModel.ErrorMessage = notLoggedInErrorLogData.ErrorMessage;
                    errorLogViewModel.IsTrue = true;
                    return errorLogViewModel;
                }
            }
            catch (Exception ex)
            {
                errorLogViewModel.IsTrue = false;
                bool error = InsertErrorToDatabase(_areaName, _controllerName, actionName, ex.ToString(), loggedInUserId);
                return errorLogViewModel;
            }
        }
    }
}
