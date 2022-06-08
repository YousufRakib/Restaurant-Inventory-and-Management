using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.CommonModel
{
    public class PurchaseMasterModel
    {
        public int Sl { get; set; }
        public int PurchaseMasterId { get; set; }

        public string Comment { get; set; }

        public string PurchaseCode { get; set; }

        public string CreatedByName { get; set; }

        public int CreatedBy { get; set; }

        public string CreatedDate { get; set; }

        public string UpdatedByName { get; set; }

        public int UpdatedBy { get; set; }

        public string UpdatedDate { get; set; }
    }
}
