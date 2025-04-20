using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace API_CINE.Models.Domain
{
    public class Seat : Entity

    {
        
        [Required, StringLength(10)]
        public string SeatNumber { get; set; }

        [Required, StringLength(10)]
        public string Row { get; set; }

        public int CinemaHallId { get; set; }
        public virtual CinemaHall CinemaHall { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
