using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.MenuModel
{
    public class ItemCategoryWithMenuViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public bool IsCheckedMenu { get; set; }
    }
}
