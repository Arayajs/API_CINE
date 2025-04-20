using API_CINE.Models.DTOs;

namespace API_CINE.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId);
        Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request);
        Task<bool> CancelOrderAsync(int orderId);
        Task<OrderDto> ProcessPaymentAsync(int orderId, string paymentMethod, string transactionId);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync(); // Add this method to the interface
    }
}
