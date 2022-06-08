using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _3DPOSRegistrationApp.Database.Model
{
    public class District
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int DistrictID { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string DistrictName { get; set; }

        [Column(TypeName = "int")]
        public int CountryID { get; set; }
    }
}
