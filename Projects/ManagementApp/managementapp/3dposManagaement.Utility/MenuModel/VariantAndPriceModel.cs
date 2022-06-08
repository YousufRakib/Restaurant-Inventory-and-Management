using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.MenuModel
{
    public class VariantAndPriceModel
    {
        public int ItemVariantId { get; set; }
        public int ItemId { get; set; }
        public int VariantId { get; set; }
        public int VariantName { get; set; }
        public int VariantMasterId { get; set; }
        public decimal VariantWiseItemprice { get; set; }
        public string RestaurantCode { get; set; }
        public string CreatedByName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedByName { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
