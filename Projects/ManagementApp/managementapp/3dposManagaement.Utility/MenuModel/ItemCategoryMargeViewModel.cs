using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.MenuModel
{
    public class ItemCategoryMargeViewModel
    {
        public List<ItemCategoryViewModel> ItemCategoryList { get; set; }
        public List<ItemCategoryWithMenuViewModel> ItemCategoryWithMenuList { get; set; }
        public ItemCategoryViewModel ItemCategoryViewModel { get; set; }

        public int Sl { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CategoryDescription { get; set; }

        public string CategoryIconPath { get; set; }

        public IFormFile ItemCategoryImageFile { get; set; }

        public string RestaurantCode { get; set; }

        public bool Status { get; set; }

        public bool IsDeleted { get; set; }

        public string CreatedByName { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string UpdatedByName { get; set; }

        public int UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
