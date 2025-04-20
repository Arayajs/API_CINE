namespace API_CINE.Models.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentTransactionId { get; set; }
        public List<TicketDto> Tickets { get; set; } = new List<TicketDto>();
    }
}
