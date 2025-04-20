using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.DTOs
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "Al menos un ítem en el carrito es obligatorio")]
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        [StringLength(100, ErrorMessage = "El método de pago no puede exceder los 100 caracteres")]
        public string PaymentMethod { get; set; }
    }
}
