using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Utility.ResturantModel
{
    public class DBCatalogViewModel
    {
        public int ID { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CanInsertRestaurant { get; set; }
        public int RestaurantCount { get; set; }
        public string SiteUrl { get; set; }
        public string MigrationFile { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDatabase_TableCreated { get; set; }
        public bool IsTryCatchError { get; set; }
    }
}
