using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.CommonModel
{
    public class PurchaseProductMargeModels
    {
        public List<PurchaseProductCategoryWiseSummation> purchaseProductCategoryWiseSummations { get; set; }
        public List<PurchaseProductViewModel> purchaseProductViewModel { get; set; }
        public string NetTotal { get; set; }
    }
}
