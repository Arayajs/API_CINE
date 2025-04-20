namespace API_CINE.Models.DTOs
{
    public class CinemaHallDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string HallType { get; set; }
        public int CinemaId { get; set; }
        public string CinemaName { get; set; }
    }
}
