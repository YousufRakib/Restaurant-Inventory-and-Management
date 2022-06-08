using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonEntityModel.EntityModel
{
    public class Resturant
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ResturantID { get; set; }

        //[UniqueKey(groupId: "1", order: 0)]
        [Column(TypeName = "nvarchar(50)")]
        public string ResturentCode { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string RestaurantName { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string CompanyRegistrationNo { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string ContactPersion { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string ContactEmail { get; set; }

        [Column(TypeName = "nvarchar(30)")]
        public string ContactPhone { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Country { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string District { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string PostCode { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string Address { get; set; }

        [Column(TypeName = "bit")]
        public bool IsDBCreated { get; set; }

        [Column(TypeName = "bit")]
        public bool IsTableCreated { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string DBCreationStatus { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string DatabaseName { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string DBConnectionString { get; set; }

        [Column(TypeName = "int")]
        public int CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "int")]
        public int UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }

        [Column(TypeName = "nvarchar(10)")]
        public string RestaurantStatus { get; set; }

        [Column(TypeName = "int")]
        public int UserStatus { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string RestaurantUserId { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Email { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Password { get; set; }

    }
}
