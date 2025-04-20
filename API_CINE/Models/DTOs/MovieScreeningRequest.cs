using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.DTOs
{
    public class MovieScreeningRequest
    {
        [Required(ErrorMessage = "El ID de la película es obligatorio")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "El ID de la sala es obligatorio")]
        public int CinemaHallId { get; set; }

        [Required(ErrorMessage = "La hora de inicio es obligatoria")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "La hora de finalización es obligatoria")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "El precio del boleto es obligatorio")]
        [Range(0.01, 10000, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal TicketPrice { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
