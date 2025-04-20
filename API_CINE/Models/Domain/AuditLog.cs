using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.Domain
{
    public class AuditLog : Entity
    {
        [Required, StringLength(50)]
        public string Action { get; set; }

        [Required, StringLength(100)]
        public string EntityName { get; set; }

        public int? EntityId { get; set; }

        [StringLength(100)]
        public string UserId { get; set; }

        [StringLength(255)]
        public string Details { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; }
    }
}
