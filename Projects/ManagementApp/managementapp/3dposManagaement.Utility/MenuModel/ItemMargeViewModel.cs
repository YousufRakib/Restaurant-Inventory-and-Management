using CommonEntityModel.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.MenuModel
{
    public class ItemMargeViewModel
    {
        public List<ItemListModel> ItemListModels{ get; set; }
        public ItemListModel ItemListModel { get; set; }
        public List<VariantAndPriceModel> VariantAndPriceModel { get; set; }
        public List<ItemWithMenuModel> ItemWithMenuModel { get; set; }
        public List<ItemWithCategoryModel> ItemWithCategoryModel { get; set; }
        public List<VariantMaster> VariantMaster { get; set; }
        public List<VariantPriceModel> VariantPriceModel { get; set; }

        public int Sl { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public string ItemImagePath { get; set; }
        public string RestaurantCode { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public bool Status { get; set; }
        public int VariantMasterId { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedByName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedByName { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
