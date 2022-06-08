using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Utility.ServiceModel
{
    public class CommonInfo
    {
        public int ErrorLogId { get; set; }
        public string AreaName { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorFromUser { get; set; }
        public string ErrorFromUserName { get; set; }
        public DateTime ErrorTime { get; set; }
        public string ErrorCode { get; set; }
        public bool IsTrue { get; set; }
    }
}
