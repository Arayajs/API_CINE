using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.Domain
{
    public class Ticket : Entity
    {
        public int OrderId { get; set; }
        public int MovieScreeningId { get; set; }
        public int SeatId { get; set; }

        public decimal Price { get; set; }

        [StringLength(100)]
        public string TicketCode { get; set; } = Guid.NewGuid().ToString("N");

        public bool IsUsed { get; set; } = false;

        public virtual Order Order { get; set; }
        public virtual MovieScreening MovieScreening { get; set; }
        public virtual Seat Seat { get; set; }
    }
}
