namespace API_CINE.Models.DTOs
{
    public class TicketDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MovieScreeningId { get; set; }
        public string MovieTitle { get; set; }
        public DateTime ScreeningTime { get; set; }
        public string CinemaName { get; set; }
        public string HallName { get; set; }
        public int SeatId { get; set; }
        public string SeatNumber { get; set; }
        public string Row { get; set; }
        public decimal Price { get; set; }
        public string TicketCode { get; set; }
        public bool IsUsed { get; set; }
    }
}
