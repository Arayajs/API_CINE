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
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IOrderService _orderService;
        private readonly IEmailService _emailService;
        private readonly ILoggingService _loggingService;

        public TicketsController(
            ITicketService ticketService,
            IOrderService orderService,
            IEmailService emailService,
            ILoggingService loggingService)
        {
            _ticketService = ticketService;
            _orderService = orderService;
            _emailService = emailService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Obtiene todos los tickets del usuario autenticado
        /// </summary>
        /// <returns>Lista de tickets del usuario</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IEnumerable<TicketDto>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetMyTickets()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var tickets = await _ticketService.GetTicketsByUserAsync(userId);

                return Ok(new ApiResponse<IEnumerable<TicketDto>>
                {
                    Success = true,
                    Message = "Tickets obtenidos con éxito",
                    Data = tickets
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener tickets del usuario: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener los tickets"
                });
            }
        }

        /// <summary>
        /// Obtiene un ticket por su ID
        /// </summary>
        /// <param name="id">ID del ticket</param>
        /// <returns>Ticket solicitado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TicketDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetTicket(int id)
        {
            try
            {
                var ticket = await _ticketService.GetTicketByIdAsync(id);
                if (ticket == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Ticket con ID {id} no encontrado"
                    });
                }

                // Verificar que el ticket pertenezca al usuario actual o sea administrador
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var order = await _orderService.GetOrderByIdAsync(ticket.OrderId);

                if (order.UserId != userId && !User.IsInRole("Administrator"))
                {
                    return Forbid();
                }

                return Ok(new ApiResponse<TicketDto>
                {
                    Success = true,
                    Message = "Ticket obtenido con éxito",
                    Data = ticket
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener ticket con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener el ticket"
                });
            }
        }

        /// <summary>
        /// Obtiene un ticket por su código
        /// </summary>
        /// <param name="code">Código del ticket</param>
        /// <returns>Ticket solicitado</returns>
        [HttpGet("code/{code}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TicketDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetTicketByCode(string code)
        {
            try
            {
                var ticket = await _ticketService.GetTicketByCodeAsync(code);
                if (ticket == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Ticket con código {code} no encontrado"
                    });
                }

                // Verificar que el ticket pertenezca al usuario actual o sea administrador
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var order = await _orderService.GetOrderByIdAsync(ticket.OrderId);

                if (order.UserId != userId && !User.IsInRole("Administrator"))
                {
                    return Forbid();
                }

                return Ok(new ApiResponse<TicketDto>
                {
                    Success = true,
                    Message = "Ticket obtenido con éxito",
                    Data = ticket
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener ticket con código {code}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener el ticket"
                });
            }
        }

        /// <summary>
        /// Valida un ticket (verifica si es válido para usar)
        /// </summary>
        /// <param name="code">Código del ticket</param>
        /// <returns>Estado de validación del ticket</returns>
        [HttpGet("validate/{code}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> ValidateTicket(string code)
        {
            try
            {
                var isValid = await _ticketService.ValidateTicketAsync(code);

                if (!isValid)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Ticket con código {code} no válido o ya utilizado"
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Ticket válido",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al validar ticket con código {code}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al validar el ticket"
                });
            }
        }

        /// <summary>
        /// Marca un ticket como usado
        /// </summary>
        /// <param name="code">Código del ticket</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("use/{code}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> UseTicket(string code)
        {
            try
            {
                var ticket = await _ticketService.GetTicketByCodeAsync(code);
                if (ticket == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Ticket con código {code} no encontrado"
                    });
                }

                // Verificar que el ticket no esté ya usado
                if (ticket.IsUsed)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Este ticket ya ha sido utilizado"
                    });
                }

                var result = await _ticketService.MarkTicketAsUsedAsync(code);

                // Registrar la acción en la auditoría
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _loggingService.LogAudit("Use", "Ticket", ticket.Id, userId,
                    $"Ticket utilizado: {code} para función {ticket.MovieTitle}",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Ticket marcado como utilizado con éxito",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al marcar ticket con código {code} como usado: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al marcar el ticket como usado"
                });
            }
        }

        /// <summary>
        /// Reenvía un ticket por correo electrónico
        /// </summary>
        /// <param name="id">ID del ticket</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{id}/resend-email")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> ResendTicketEmail(int id)
        {
            try
            {
                var ticket = await _ticketService.GetTicketByIdAsync(id);
                if (ticket == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Ticket con ID {id} no encontrado"
                    });
                }

                // Verificar que el ticket pertenezca al usuario actual o sea administrador
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var order = await _orderService.GetOrderByIdAsync(ticket.OrderId);

                if (order.UserId != userId && !User.IsInRole("Administrator"))
                {
                    return Forbid();
                }

                // Enviar correo con el ticket
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                await _emailService.SendTicketEmailAsync(ticket, userEmail);

                // Registrar la acción en la auditoría
                _loggingService.LogAudit("ResendEmail", "Ticket", id, userId.ToString(),
                    $"Email reenviado para ticket ID {id}",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Email con el ticket reenviado con éxito",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al reenviar email para ticket con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al reenviar el email con el ticket"
                });
            }
        }
    }
}