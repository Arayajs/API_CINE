using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using API_CINE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_CINE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Este controlador requiere autenticación para todos los endpoints
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ITicketService _ticketService;
        private readonly IPaymentService _paymentService;
        private readonly IEmailService _emailService;
        private readonly ILoggingService _loggingService;

        public OrdersController(
            IOrderService orderService,
            ITicketService ticketService,
            IPaymentService paymentService,
            IEmailService emailService,
            ILoggingService loggingService)
        {
            _orderService = orderService;
            _ticketService = ticketService;
            _paymentService = paymentService;
            _emailService = emailService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Obtiene todas las órdenes del usuario autenticado
        /// </summary>
        /// <returns>Lista de órdenes del usuario</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IEnumerable<OrderDto>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var orders = await _orderService.GetOrdersByUserAsync(userId);

                return Ok(new ApiResponse<IEnumerable<OrderDto>>
                {
                    Success = true,
                    Message = "Órdenes obtenidas con éxito",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener órdenes del usuario: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener las órdenes"
                });
            }
        }

        /// <summary>
        /// Obtiene una orden por su ID
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <returns>Orden solicitada</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<OrderDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetOrder(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Orden con ID {id} no encontrada"
                    });
                }

                // Verificar que la orden pertenezca al usuario actual o sea administrador
                if (order.UserId != userId && !User.IsInRole("Administrator"))
                {
                    return Forbid();
                }

                return Ok(new ApiResponse<OrderDto>
                {
                    Success = true,
                    Message = "Orden obtenida con éxito",
                    Data = order
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener orden con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener la orden"
                });
            }
        }

        /// <summary>
        /// Obtiene todos los boletos de una orden
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <returns>Lista de boletos de la orden</returns>
        [HttpGet("{id}/tickets")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IEnumerable<TicketDto>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetOrderTickets(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Orden con ID {id} no encontrada"
                    });
                }

                // Verificar que la orden pertenezca al usuario actual o sea administrador
                if (order.UserId != userId && !User.IsInRole("Administrator"))
                {
                    return Forbid();
                }

                var tickets = await _ticketService.GetTicketsByOrderAsync(id);
                return Ok(new ApiResponse<IEnumerable<TicketDto>>
                {
                    Success = true,
                    Message = "Boletos obtenidos con éxito",
                    Data = tickets
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener boletos para orden con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener los boletos de la orden"
                });
            }
        }

        /// <summary>
        /// Crea una nueva orden (compra de entradas)
        /// </summary>
        /// <param name="orderRequest">Datos de la orden</param>
        /// <returns>Orden creada</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<OrderDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest orderRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Datos de orden inválidos",
                        Data = ModelState
                    });
                }

                if (orderRequest.CartItems.Count == 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "La orden debe contener al menos un boleto"
                    });
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var order = await _orderService.CreateOrderAsync(userId, orderRequest);

                // Procesar el pago si se proporcionó información de pago
                if (!string.IsNullOrEmpty(orderRequest.PaymentMethod))
                {
                    await _orderService.ProcessPaymentAsync(order.Id, orderRequest.PaymentMethod, null);
                }

                // Registrar la acción en la auditoría
                _loggingService.LogAudit("Create", "Order", order.Id, userId.ToString(),
                    $"Orden creada: {order.Id} con {order.Tickets.Count} boletos",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                // Enviar correo de confirmación
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                await _emailService.SendOrderConfirmationAsync(order, userEmail);

                return Created($"/api/orders/{order.Id}", new ApiResponse<OrderDto>
                {
                    Success = true,
                    Message = "Orden creada con éxito",
                    Data = order
                });
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al crear orden: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al crear orden: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al crear la orden"
                });
            }
        }

        /// <summary>
        /// Procesa el pago de una orden
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <param name="paymentRequest">Información del pago</param>
        /// <returns>Orden procesada</returns>
        [HttpPost("{id}/process-payment")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<OrderDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> ProcessPayment(int id, [FromBody] PaymentRequest paymentRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Datos de pago inválidos",
                        Data = ModelState
                    });
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Orden con ID {id} no encontrada"
                    });
                }

                // Verificar que la orden pertenezca al usuario actual
                if (order.UserId != userId)
                {
                    return Forbid();
                }

                // Verificar que la orden no haya sido pagada previamente
                if (order.Status != "Pending")
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Esta orden ya ha sido procesada"
                    });
                }

                // Procesar el pago
                var paymentResult = await _paymentService.ProcessPaymentAsync(paymentRequest);
                if (!paymentResult.Success)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = paymentResult.Message
                    });
                }

                // Actualizar la orden con la información del pago
                var updatedOrder = await _orderService.ProcessPaymentAsync(id, paymentRequest.PaymentMethod, paymentResult.TransactionId);

                // Registrar la acción en la auditoría
                _loggingService.LogAudit("Payment", "Order", id, userId.ToString(),
                    $"Pago procesado para orden {id}: {paymentResult.TransactionId}",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                // Enviar correo de confirmación
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                await _emailService.SendOrderConfirmationAsync(updatedOrder, userEmail);

                return Ok(new ApiResponse<OrderDto>
                {
                    Success = true,
                    Message = "Pago procesado con éxito",
                    Data = updatedOrder
                });
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al procesar pago para orden con ID {id}: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al procesar pago para orden con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al procesar el pago de la orden"
                });
            }
        }

        /// <summary>
        /// Cancela una orden
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Orden con ID {id} no encontrada"
                    });
                }

                // Verificar que la orden pertenezca al usuario actual o sea administrador
                if (order.UserId != userId && !User.IsInRole("Administrator"))
                {
                    return Forbid();
                }

                // Verificar que la orden pueda ser cancelada
                if (order.Status != "Pending" && order.Status != "Completed")
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Esta orden no puede ser cancelada"
                    });
                }

                var result = await _orderService.CancelOrderAsync(id);

                // Registrar la acción en la auditoría
                _loggingService.LogAudit("Cancel", "Order", id, userId.ToString(),
                    $"Orden cancelada: {id}",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Orden cancelada con éxito",
                    Data = result
                });
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al cancelar orden con ID {id}: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al cancelar orden con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al cancelar la orden"
                });
            }
        }

        /// <summary>
        /// Obtiene todas las órdenes (solo para administradores)
        /// </summary>
        /// <returns>Lista de todas las órdenes</returns>
        [HttpGet("all")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IEnumerable<OrderDto>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                // Este endpoint solo está disponible para administradores
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(new ApiResponse<IEnumerable<OrderDto>>
                {
                    Success = true,
                    Message = "Órdenes obtenidas con éxito",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener todas las órdenes: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener todas las órdenes"
                });
            }
        }
    }
}