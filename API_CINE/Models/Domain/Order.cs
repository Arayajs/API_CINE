using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace API_CINE.Models.Domain
{
    public class Order : Entity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string Status { get; set; } // Pending, Completed, Cancelled, etc.

        [StringLength(100)]
        public string PaymentMethod { get; set; }

        [StringLength(200)]
        public string PaymentTransactionId { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
