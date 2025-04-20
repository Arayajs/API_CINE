using System.ComponentModel.DataAnnotations;

namespace API_CINE.Models.Domain
{
    public class User : Entity
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
