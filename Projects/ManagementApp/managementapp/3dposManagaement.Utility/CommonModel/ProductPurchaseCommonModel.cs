using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.CommonModel
{
    public class ProductPurchaseCommonModel
    {
        public PurchaseMasterModel PurchaseMasterModel { get; set; }
        public List<PurchaseProductSaveModel> PurchaseProductSaveModel { get; set; }
    }
}
