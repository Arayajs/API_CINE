using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.DTOs
{
    public class CinemaHallRequest
    {

        [Required(ErrorMessage = "El nombre de la sala es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "La capacidad es obligatoria")]
        [Range(1, 1000, ErrorMessage = "La capacidad debe estar entre 1 y 1000")]
        public int Capacity { get; set; }

        [StringLength(100, ErrorMessage = "El tipo de sala no puede exceder los 100 caracteres")]
        public string HallType { get; set; }

        [Required(ErrorMessage = "El ID del cine es obligatorio")]
        public int CinemaId { get; set; }
    }
}
