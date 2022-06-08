using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _3DPOSRegistrationApp.Database.Model
{
    public class Country
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CountryID { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string CountryName { get; set; }
    }
}
