using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.DTOs
{
    public class MovieRequest
    {

        [Required(ErrorMessage = "El título de la película es obligatorio")]
        [StringLength(100, ErrorMessage = "El título no puede exceder los 100 caracteres")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string Description { get; set; }

        [Required(ErrorMessage = "La duración es obligatoria")]
        public TimeSpan Duration { get; set; }

        [StringLength(50, ErrorMessage = "El género no puede exceder los 50 caracteres")]
        public string Genre { get; set; }

        [StringLength(100, ErrorMessage = "El director no puede exceder los 100 caracteres")]
        public string Director { get; set; }

        [StringLength(255, ErrorMessage = "La URL de la imagen no puede exceder los 255 caracteres")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "La fecha de estreno es obligatoria")]
        public DateTime ReleaseDate { get; set; }

        [StringLength(5, ErrorMessage = "La clasificación no puede exceder los 5 caracteres")]
        public string Rating { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
