using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.CommonModel
{
    public class UniteTypeViewModel
    {
        public int UnitTypeId { get; set; }

        public string UnitTypeName { get; set; }

        public string RestaurantCode { get; set; }

        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string CreatedDate { get; set; }

        public int UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }

        public string UpdatedDate { get; set; }
    }
}
