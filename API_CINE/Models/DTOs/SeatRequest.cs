using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.DTOs
{
    public class SeatRequest
    {
        [Required(ErrorMessage = "El número de asiento es obligatorio")]
        [StringLength(10, ErrorMessage = "El número de asiento no puede exceder los 10 caracteres")]
        public string SeatNumber { get; set; }

        [Required(ErrorMessage = "La fila es obligatoria")]
        [StringLength(10, ErrorMessage = "La fila no puede exceder los 10 caracteres")]
        public string Row { get; set; }

        [Required(ErrorMessage = "El ID de la sala es obligatorio")]
        public int CinemaHallId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
