using CommonEntityModel.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.CommonModel
{
    public class InventoryViewModel
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }

        public InventoryModel inventory { get; set; }
        public List<Product> products { get; set; }
        public string ProductName { get; set; }

        public string RestaurantCode { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal Total { get; set; }

        public string Comment { get; set; }

        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }

        public string CreatedDate { get; set; }

        public int UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }

        public string UpdatedDate { get; set; }
    }
}
