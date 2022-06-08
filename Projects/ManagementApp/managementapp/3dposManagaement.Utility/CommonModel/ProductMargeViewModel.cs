using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.CommonModel
{
    public class ProductMargeViewModel
    {
        public List<ProductModel> productModels { get; set; }

        public int SI { get; set; }
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string RestaurantCode { get; set; }

        public int ProductCategoryId { get; set; }
        public string ProductCategory { get; set; }

        public int UnitTypeId { get; set; }
        public string UnitType { get; set; }

        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }

        public string CreatedDate { get; set; }

        public int UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }

        public string UpdatedDate { get; set; }
    }
}
