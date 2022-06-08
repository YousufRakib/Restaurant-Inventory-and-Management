using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonEntityModel.EntityModel
{
    public class ErrorLog
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ErrorLogId { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string ErrorCode { get; set; }
        public DateTime ErrorTime { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string ControllerName { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string ActionName { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string ErrorMessage { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string AreaName { get; set; }

        [Column(TypeName = "int")]
        public int ErrorFromUser { get; set; }
    }
}
