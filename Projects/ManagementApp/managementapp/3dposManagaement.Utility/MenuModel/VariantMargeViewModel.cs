using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.MenuModel
{
    public class VariantMargeViewModel
    {
        public List<VariantViewModel> VariantViewModelList { get; set; }
        public VariantViewModel VariantViewModel { get; set; }

        public int VariantMasterId { get; set; }
        public string VariantMasterName { get; set; }
        public string VariantName { get; set; }
        public int VariantMasterNumber { get; set; }
        public string CreatedByName { get; set; }
        public string RestaurantCode { get; set; }
        public bool Status { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedByName { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
