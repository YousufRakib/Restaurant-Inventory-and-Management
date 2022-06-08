using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonEntityModel.EntityModel
{
    public class AuditLog
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int AuditLogId { get; set; }

        [Column(TypeName = "int")]
        public int AuditBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? AuditTime { get; set; }

        [Column(TypeName = "nvarchar(1000)")]
        public string PreviousInformation { get; set; }

        [Column(TypeName = "nvarchar(1000)")]
        public string UpdatedInformation { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string ControllerName { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string ActionName { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string AreaName { get; set; }

    }
}
