using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.DTOs
{
    public class UserRoleRequest
    {
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        public string RoleName { get; set; }
    }

}
