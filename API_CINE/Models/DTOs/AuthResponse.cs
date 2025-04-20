namespace API_CINE.Models.DTOs
{
    public class AuthResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string Token { get; set; }
        public DateTime TokenExpiration { get; set; }
    }
}
