using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.Domain
{
    public class Cinema : Entity
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(200)]
        public string Address { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required, StringLength(100)]
        public string City { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<CinemaHall> CinemaHalls { get; set; } = new List<CinemaHall>();
    }
}
