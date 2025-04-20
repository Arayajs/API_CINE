namespace API_CINE.Models.DTOs
{
    public class SeatDto
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; }
        public string Row { get; set; }
        public int CinemaHallId { get; set; }
        public bool IsAvailable { get; set; }
    }
}
