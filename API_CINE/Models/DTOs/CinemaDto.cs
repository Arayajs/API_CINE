namespace API_CINE.Models.DTOs
{
    public class CinemaDto
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public bool IsActive { get; set; }
        public List<CinemaHallDto> Halls { get; set; } = new List<CinemaHallDto>();
    }

}
