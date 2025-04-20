namespace API_CINE.Models.DTOs
{
    public class MovieDto
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public string ImageUrl { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Rating { get; set; }
        public bool IsActive { get; set; }
    }
}
