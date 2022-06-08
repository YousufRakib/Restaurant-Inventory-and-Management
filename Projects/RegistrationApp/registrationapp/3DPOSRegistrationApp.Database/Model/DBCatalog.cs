using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _3DPOSRegistrationApp.Database.Model
{
    public class DBCatalog
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }

        [Column(TypeName = "nvarchar(300)")]
        public string ConnectionString { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string DatabaseName { get; set; }

        public int RestaurantCount { get; set; }

        public bool CanInsertRestaurant { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string SiteUrl { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsDBCreated { get; set; }
        public string MigrationFile { get; set; }

        [Column(TypeName = "int")]
        public int CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "int")]
        public int UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
    }
}
