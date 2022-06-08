using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.CommonModel
{
    public class PurchaseProductMargeModelsForMaster
    {
        public List<PurchaseProductCategoryWiseSummation> purchaseProductCategoryWiseSummations { get; set; }
        public List<PurchaseMasterModel> purchaseMasterModel { get; set; }
        public string NetTotal { get; set; }
    }
}
