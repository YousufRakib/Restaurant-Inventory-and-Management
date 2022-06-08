using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.CommonModel
{
    public class PurchaseProductViewModel
    {
        public int Sl { get; set; }
        public int PurchaseId { get; set; }
        public string PurchaseCode { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public string ProductName { get; set; }
        public string ProductCategoryName { get; set; }
        public string UnitTypeName { get; set; }
        public string PurchaseFrom { get; set; }
        public string CreatedDate { get; set; }
    }
}
