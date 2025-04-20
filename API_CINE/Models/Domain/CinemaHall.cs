using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.Domain
{
    public class CinemaHall : Entity
    {
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string HallType { get; set; }
        public int CinemaId { get; set; }
        public bool IsActive { get; set; } // Add this property
        public virtual Cinema Cinema { get; set; }
        public virtual ICollection<MovieScreening> MovieScreenings { get; set; }
        public virtual ICollection<Seat> Seats { get; set; }
    }
}
