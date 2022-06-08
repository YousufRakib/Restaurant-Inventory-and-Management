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
    public class ErrorLogRepository : IErrorLogRepository
    {
        private const string _areaName = "3DPOSManagement Module";
        private const string _controllerName = "ErrorLogRepository";
        private readonly RestaurantManagementDBContext _DBContext;

        public ErrorLogRepository(RestaurantManagementDBContext DBContext)/*, IConfiguration configuration)*/
        {
            _DBContext = DBContext;
        }

        public bool InsertErrorToDatabase(string areaName, string controllerName, string actionName, string errorMessage, int errorFromUser)
        {
            try
            {
                var error = new ErrorLog();
                int errorId = _DBContext.ErrorLog.Max(rq => (int?)rq.ErrorLogId) ?? 0;
                errorId++;

                var rqStr = errorId.ToString().PadLeft(6, '0');
                string errorCode = "3DPOS" + DateTime.Now.Year + "-" + rqStr;
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
    }
}
