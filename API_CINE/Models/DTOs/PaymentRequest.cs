using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.DTOs
{
    public class PaymentRequest
    {
        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, 100000, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        [StringLength(100, ErrorMessage = "El método de pago no puede exceder los 100 caracteres")]
        public string PaymentMethod { get; set; }

        [StringLength(16, MinimumLength = 16, ErrorMessage = "El número de tarjeta debe tener 16 dígitos")]
        public string CardNumber { get; set; }

        [StringLength(5, ErrorMessage = "La fecha de expiración no puede exceder los 5 caracteres")]
        public string ExpiryDate { get; set; }

        [StringLength(3, MinimumLength = 3, ErrorMessage = "El CVV debe tener 3 dígitos")]
        public string Cvv { get; set; }
    }
}
