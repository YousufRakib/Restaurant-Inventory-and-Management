using CommonEntityModel.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.MenuModel
{
    public class ItemListModel
    {
        public int Sl { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string Menus { get; set; }
        public string Categories { get; set; }
        public string ItemDescription { get; set; }
        public string ItemImagePath { get; set; }
        public string RestaurantCode { get; set; }
        public List<ItemVariant> VariantAndPriceModel { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public bool Status { get; set; }
        public string VariantMasterName { get; set; }
        public string VariantMasterId { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedByName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedByName { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
