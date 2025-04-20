namespace API_CINE.Models.DTOs
{
    public class MovieScreeningDto
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public string MovieImageUrl { get; set; }
        public int CinemaHallId { get; set; }
        public string CinemaHallName { get; set; }
        public int CinemaId { get; set; }
        public string CinemaName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TicketPrice { get; set; }
        public bool IsActive { get; set; }
        public int AvailableSeats { get; set; }
    }
}
