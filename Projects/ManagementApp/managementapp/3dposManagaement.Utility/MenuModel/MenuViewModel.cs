using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.MenuModel
{
    public class MenuViewModel
    {
        public int Sl { get; set; }

        public int MenuId { get; set; }

        public string MenuName { get; set; }

        public string MenuDescription { get; set; }

        public string MenuImagePath { get; set; }
        public IFormFile MenuImageFile { get; set; }

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
