using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.MenuModel
{
    public class MenuLogModel
    {
        public int Sl { get; set; }
        public int Id { get; set; }
        public string ChangeType { get; set; }
        public string ChangeItemName { get; set; }
        public string OldValue { get; set; }
        public string UpdatedValue { get; set; }
        public string ChangedObject { get; set; }
        public string RestaurantCode { get; set; }
        public string CreatedByName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedByName { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
