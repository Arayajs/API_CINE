using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.DTOs
{
    public class UserUpdateRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name { get; set; }

        [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder los 100 caracteres")]
        public string Email { get; set; }
    }
}
