using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.DTOs
{
    public class RoleRequest
    {
        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre del rol no puede exceder los 50 caracteres")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        public string Description { get; set; }
    }
}
