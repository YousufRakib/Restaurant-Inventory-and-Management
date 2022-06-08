using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Repository.IRepository.ILogRepository
{
    public interface IErrorLogRepository
    {
        bool InsertErrorToDatabase(string areaName, string controllerName, string actionName, string errorMessage, int errorFromUser);
    }
}
