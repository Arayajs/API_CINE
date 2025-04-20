using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.Domain
{
    public class Movie : Entity
    {
        [Required, StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public TimeSpan Duration { get; set; }

        [StringLength(50)]
        public string Genre { get; set; }

        [StringLength(100)]
        public string Director { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }

        public DateTime ReleaseDate { get; set; }

        [StringLength(5)]
        public string Rating { get; set; } // G, PG, PG-13, R, etc.

        public bool IsActive { get; set; } = true;

        public virtual ICollection<MovieScreening> MovieScreenings { get; set; } = new List<MovieScreening>();
    }
}
