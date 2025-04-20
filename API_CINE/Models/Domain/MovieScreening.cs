using System.Net.Sockets;

namespace API_CINE.Models.Domain
{
    public class MovieScreening : Entity
    {
        public int MovieId { get; set; }
        public int CinemaHallId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public decimal TicketPrice { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual Movie Movie { get; set; }
        public virtual CinemaHall CinemaHall { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
