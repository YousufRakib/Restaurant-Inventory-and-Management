using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.CommonModel
{
    public class PurchaseProductSaveModel
    {
        public string PurchaseCode { get; set; }
        public int ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int UnitTypeId { get; set; }
        public string UnitTypeName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public string PurchasedFrom { get; set; }
    }
}
