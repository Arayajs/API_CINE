using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.DTOs
{
    public class CartItemDto
    {
        [Required(ErrorMessage = "El ID de la proyección es obligatorio")]
        public int MovieScreeningId { get; set; }

        [Required(ErrorMessage = "El ID del asiento es obligatorio")]
        public int SeatId { get; set; }
    }
}
