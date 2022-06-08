using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonEntityModel.EntityModel
{
    public class RoleUpdateHistory
    {

        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "int")]
        public int UserId { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Username { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Email { get; set; }

        //[Column(TypeName = "int")]
        [Column(TypeName = "nvarchar(20)")]
        public string PreviousRole { get; set; }

        //[Column(TypeName = "int")]
        [Column(TypeName = "nvarchar(20)")]
        public string UpdatedRole { get; set; }

        [Column(TypeName = "int")]
        public int UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime UpdatedDate { get; set; }
    }
}
