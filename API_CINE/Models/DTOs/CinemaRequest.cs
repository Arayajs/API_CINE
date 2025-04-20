using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.DTOs
{
    public class CinemaRequest
    {
        [Required(ErrorMessage = "El nombre del cine es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string Address { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string Description { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        [StringLength(100, ErrorMessage = "La ciudad no puede exceder los 100 caracteres")]
        public string City { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
