using API_CINE.Models.DTOs;
using API_CINE.Repositories.Interfaces;
using API_CINE.Services.Interfaces;
using AutoMapper;

namespace API_CINE.Services.Implementations
{
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;

        public TicketService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggingService loggingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _loggingService = loggingService;
        }

        public async Task<TicketDto> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
                if (ticket == null)
                    return null;

                // Obtener detalles completos del ticket
                var ticketWithDetails = await _unitOfWork.Tickets.GetTicketByCodeAsync(ticket.TicketCode);
                return MapTicketToDto(ticketWithDetails);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener ticket por ID {ticketId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<TicketDto> GetTicketByCodeAsync(string ticketCode)
        {
            try
            {
                var ticket = await _unitOfWork.Tickets.GetTicketByCodeAsync(ticketCode);
                if (ticket == null)
                    return null;

                return MapTicketToDto(ticket);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener ticket por código {ticketCode}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<TicketDto>> GetTicketsByOrderAsync(int orderId)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                if (order == null)
                    throw new ApplicationException($"Orden con ID {orderId} no encontrada");

                var tickets = await _unitOfWork.Tickets.GetTicketsByOrderAsync(orderId);
                var ticketDtos = new List<TicketDto>();

                foreach (var ticket in tickets)
                {
                    ticketDtos.Add(MapTicketToDto(ticket));
                }

                return ticketDtos;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al obtener tickets para orden {orderId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener tickets para orden {orderId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<TicketDto>> GetTicketsByUserAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new ApplicationException($"Usuario con ID {userId} no encontrado");

                var tickets = await _unitOfWork.Tickets.GetTicketsByUserAsync(userId);
                var ticketDtos = new List<TicketDto>();

                foreach (var ticket in tickets)
                {
                    ticketDtos.Add(MapTicketToDto(ticket));
                }

                return ticketDtos;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al obtener tickets para usuario {userId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener tickets para usuario {userId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<bool> ValidateTicketAsync(string ticketCode)
        {
            try
            {
                var ticket = await _unitOfWork.Tickets.GetTicketByCodeAsync(ticketCode);
                if (ticket == null)
                    return false;

                // Verificar si el ticket ya fue utilizado
                if (ticket.IsUsed)
                    return false;

                // Verificar que la proyección no haya finalizado
                if (ticket.MovieScreening.EndTime < DateTime.Now)
                    return false;

                // Verificar que la orden esté completada
                if (ticket.Order.Status != "Completed")
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al validar ticket con código {ticketCode}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<bool> MarkTicketAsUsedAsync(string ticketCode)
        {
            try
            {
                var ticket = await _unitOfWork.Tickets.GetTicketByCodeAsync(ticketCode);
                if (ticket == null)
                    throw new ApplicationException($"Ticket con código {ticketCode} no encontrado");

                // Verificar si el ticket ya fue utilizado
                if (ticket.IsUsed)
                    throw new ApplicationException("Este ticket ya ha sido utilizado");

                // Verificar que la proyección no haya finalizado
                if (ticket.MovieScreening.EndTime < DateTime.Now)
                    throw new ApplicationException("La proyección ya ha finalizado");

                // Verificar que la orden esté completada
                if (ticket.Order.Status != "Completed")
                    throw new ApplicationException("La orden asociada a este ticket no está completada");

                // Marcar como utilizado
                ticket.IsUsed = true;
                await _unitOfWork.Tickets.UpdateAsync(ticket);
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al marcar ticket {ticketCode} como usado: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al marcar ticket {ticketCode} como usado: {ex.Message}", ex);
                throw;
            }
        }

        // Método auxiliar para mapear un ticket a DTO
        private TicketDto MapTicketToDto(Models.Domain.Ticket ticket)
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

            return ticketDto;
        }
    }
}