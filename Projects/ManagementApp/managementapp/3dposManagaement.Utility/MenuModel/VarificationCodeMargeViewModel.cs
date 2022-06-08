using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dposManagaement.Utility.MenuModel
{
    public class VarificationCodeMargeViewModel
    {
        public List<VarificationCodeViewModel> varificationCodeViewModel { get; set; }
        public int SI { get; set; }
        public int Id { get; set; }

        public string Code { get; set; }

        public string Username { get; set; }
        public int UserId { get; set; }

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
