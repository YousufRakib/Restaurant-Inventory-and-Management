using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Utility.LogViewModel
{
    public class AuditLogViewModel
    {
        public int AuditLogId { get; set; }
        public string AuditBy { get; set; }
        public DateTime? AuditTime { get; set; }
        public string PreviousInformation { get; set; }
        public string UpdatedInformation { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string AreaName { get; set; }
    }
}
