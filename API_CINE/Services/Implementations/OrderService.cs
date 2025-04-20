using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using API_CINE.Repositories.Interfaces;
using API_CINE.Services.Interfaces;
using AutoMapper;

namespace API_CINE.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;

        public OrderService(
            IUnitOfWork unitOfWork,
            IPaymentService paymentService,
            IEmailService emailService,
            IMapper mapper,
            ILoggingService loggingService)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _emailService = emailService;
            _mapper = mapper;
            _loggingService = loggingService;
        }

        public async Task<OrderDto> GetOrderByIdAsync(int orderId)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                    return null;

                return MapOrderToDto(order);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener orden por ID {orderId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new ApplicationException($"Usuario con ID {userId} no encontrado");

                var orders = await _unitOfWork.Orders.GetOrdersByUserAsync(userId);
                var orderDtos = new List<OrderDto>();

                foreach (var order in orders)
                {
                    orderDtos.Add(MapOrderToDto(order));
                }

                return orderDtos;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al obtener órdenes para usuario {userId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener órdenes para usuario {userId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            try
            {
                var orders = await _unitOfWork.Orders.GetAllAsync();
                var orderDtos = new List<OrderDto>();

                foreach (var order in orders)
                {
                    // Obtener detalles completos de la orden
                    var orderWithDetails = await _unitOfWork.Orders.GetOrderWithDetailsAsync(order.Id);
                    orderDtos.Add(MapOrderToDto(orderWithDetails));
                }

                return orderDtos;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener todas las órdenes: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new ApplicationException($"Usuario con ID {userId} no encontrado");

                // Validar que haya items en el carrito
                if (request.CartItems == null || !request.CartItems.Any())
                    throw new ApplicationException("La orden debe contener al menos un boleto");

                // Crear la orden
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending", // Estado inicial
                    PaymentMethod = request.PaymentMethod
                };

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.CompleteAsync();

                // Procesar cada item del carrito
                decimal totalAmount = 0;
                foreach (var cartItem in request.CartItems)
                {
                    // Validar la proyección
                    var screening = await _unitOfWork.MovieScreenings.GetByIdAsync(cartItem.MovieScreeningId);
                    if (screening == null)
                        throw new ApplicationException($"Proyección con ID {cartItem.MovieScreeningId} no encontrada");

                    if (!screening.IsActive)
                        throw new ApplicationException($"La proyección con ID {cartItem.MovieScreeningId} no está activa");

                    // Validar que la proyección no haya pasado
                    if (screening.StartTime <= DateTime.Now)
                        throw new ApplicationException($"La proyección con ID {cartItem.MovieScreeningId} ya ha comenzado o finalizado");

                    // Validar el asiento
                    var seat = await _unitOfWork.Seats.GetByIdAsync(cartItem.SeatId);
                    if (seat == null)
                        throw new ApplicationException($"Asiento con ID {cartItem.SeatId} no encontrado");

                    if (!seat.IsActive)
                        throw new ApplicationException($"El asiento con ID {cartItem.SeatId} no está activo");

                    // Validar que el asiento pertenezca a la sala de la proyección
                    if (seat.CinemaHallId != screening.CinemaHallId)
                        throw new ApplicationException($"El asiento con ID {cartItem.SeatId} no pertenece a la sala de la proyección");

                    // Validar que el asiento esté disponible para esta proyección
                    bool isAvailable = await _unitOfWork.Seats.IsSeatAvailableForScreeningAsync(cartItem.SeatId, cartItem.MovieScreeningId);
                    if (!isAvailable)
                        throw new ApplicationException($"El asiento con ID {cartItem.SeatId} no está disponible para esta proyección");

                    // Crear el ticket
                    var ticket = new Ticket
                    {
                        OrderId = order.Id,
                        MovieScreeningId = cartItem.MovieScreeningId,
                        SeatId = cartItem.SeatId,
                        Price = screening.TicketPrice,
                        TicketCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(), // Código único para el ticket
                        IsUsed = false
                    };

                    await _unitOfWork.Tickets.AddAsync(ticket);
                    totalAmount += ticket.Price;
                }

                // Actualizar el monto total de la orden
                order.TotalAmount = totalAmount;
                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.CompleteAsync();

                // Obtener la orden con todos los detalles
                var createdOrder = await _unitOfWork.Orders.GetOrderWithDetailsAsync(order.Id);
                return MapOrderToDto(createdOrder);
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al crear orden para usuario {userId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al crear orden para usuario {userId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<OrderDto> ProcessPaymentAsync(int orderId, string paymentMethod, string transactionId)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                    throw new ApplicationException($"Orden con ID {orderId} no encontrada");

                // Verificar que la orden esté pendiente
                if (order.Status != "Pending")
                    throw new ApplicationException("Solo se pueden procesar órdenes pendientes");

                // Actualizar información de pago
                order.Status = "Completed"; // Orden completada
                order.PaymentMethod = paymentMethod;
                order.PaymentTransactionId = transactionId;

                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.CompleteAsync();

                // Obtener la orden actualizada
                var updatedOrder = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
                return MapOrderToDto(updatedOrder);
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al procesar pago para orden {orderId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al procesar pago para orden {orderId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                    throw new ApplicationException($"Orden con ID {orderId} no encontrada");

                // Verificar que la orden pueda ser cancelada
                if (order.Status != "Pending" && order.Status != "Completed")
                    throw new ApplicationException("Esta orden no puede ser cancelada");

                // Verificar que ninguna de las proyecciones haya comenzado
                foreach (var ticket in order.Tickets)
                {
                    if (ticket.MovieScreening.StartTime <= DateTime.Now)
                        throw new ApplicationException("No se puede cancelar una orden con proyecciones que ya han comenzado");
                }

                // Cancelar la orden
                order.Status = "Cancelled";
                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al cancelar orden {orderId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al cancelar orden {orderId}: {ex.Message}", ex);
                throw;
            }
        }

        // Método auxiliar para mapear una orden a DTO
        private OrderDto MapOrderToDto(Order order)
        {
            var orderDto = _mapper.Map<OrderDto>(order);

            if (order.User != null)
            {
                orderDto.UserName = order.User.Name;
            }

            // Mapear tickets si existen
            if (order.Tickets != null)
            {
                foreach (var ticket in order.Tickets)
                {
                    var ticketDto = _mapper.Map<TicketDto>(ticket);

                    // Completar información adicional
                    if (ticket.MovieScreening != null)
                    {
                        ticketDto.MovieTitle = ticket.MovieScreening.Movie?.Title;
                        ticketDto.ScreeningTime = ticket.MovieScreening.StartTime;

                        if (ticket.MovieScreening.CinemaHall != null)
                        {
                            ticketDto.HallName = ticket.MovieScreening.CinemaHall.Name;

                            if (ticket.MovieScreening.CinemaHall.Cinema != null)
                            {
                                ticketDto.CinemaName = ticket.MovieScreening.CinemaHall.Cinema.Name;
                            }
                        }
                    }

                    if (ticket.Seat != null)
                    {
                        ticketDto.Row = ticket.Seat.Row;
                        ticketDto.SeatNumber = ticket.Seat.SeatNumber;
                    }

                    orderDto.Tickets.Add(ticketDto);
                }
            }

            return orderDto;
        }
    }
}

