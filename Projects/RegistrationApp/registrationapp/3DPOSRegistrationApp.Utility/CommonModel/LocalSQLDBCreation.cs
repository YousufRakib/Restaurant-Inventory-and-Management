using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Utility.CommonModel
{
    public class LocalSQLDBCreation
    {
        public string DataSource = "DESKTOP-TPUFUO1\\SQLEXPRESS01";
        public string UserId = "sa";
        public string Password = "123456";
        public string Database { get; set; }
        public string ConnectionString { get; set; }
    }
}
