using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using API_CINE.Repositories.Interfaces;
using API_CINE.Services.Interfaces;
using AutoMapper;

namespace API_CINE.Services.Implementations
{
    public class SeatService : ISeatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;

        public SeatService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggingService loggingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _loggingService = loggingService;
        }

        public async Task<SeatDto> GetSeatByIdAsync(int seatId)
        {
            try
            {
                var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
                if (seat == null)
                    return null;

                var result = _mapper.Map<SeatDto>(seat);

                // Por defecto, asumimos que el asiento está disponible
                result.IsAvailable = true;

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener asiento por ID {seatId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<SeatDto>> GetSeatsByCinemaHallAsync(int hallId)
        {
            try
            {
                var hall = await _unitOfWork.CinemaHalls.GetByIdAsync(hallId);
                if (hall == null)
                    throw new ApplicationException($"Sala con ID {hallId} no encontrada");

                var seats = await _unitOfWork.Seats.GetSeatsByCinemaHallAsync(hallId);
                var seatDtos = _mapper.Map<IEnumerable<SeatDto>>(seats);

                // Por defecto, asumimos que todos los asientos están disponibles
                foreach (var seat in seatDtos)
                {
                    seat.IsAvailable = true;
                }

                return seatDtos;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al obtener asientos por sala {hallId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener asientos por sala {hallId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<SeatDto>> GetAvailableSeatsForScreeningAsync(int screeningId)
        {
            try
            {
                var screening = await _unitOfWork.MovieScreenings.GetByIdAsync(screeningId);
                if (screening == null)
                    throw new ApplicationException($"Proyección con ID {screeningId} no encontrada");

                var seats = await _unitOfWork.Seats.GetAvailableSeatsForScreeningAsync(screeningId);
                var seatDtos = _mapper.Map<IEnumerable<SeatDto>>(seats);

                // Todos los asientos en esta lista son disponibles
                foreach (var seat in seatDtos)
                {
                    seat.IsAvailable = true;
                }

                return seatDtos;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al obtener asientos disponibles para proyección {screeningId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener asientos disponibles para proyección {screeningId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<SeatDto> CreateSeatAsync(SeatRequest request)
        {
            try
            {
                var hall = await _unitOfWork.CinemaHalls.GetByIdAsync(request.CinemaHallId);
                if (hall == null)
                    throw new ApplicationException($"Sala con ID {request.CinemaHallId} no encontrada");

                // Verificar si el asiento ya existe en la sala
                var existingSeats = await _unitOfWork.Seats.GetSeatsByCinemaHallAsync(request.CinemaHallId);
                foreach (var existingSeat in existingSeats)
                {
                    if (existingSeat.Row.Equals(request.Row, StringComparison.OrdinalIgnoreCase) &&
                        existingSeat.SeatNumber.Equals(request.SeatNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ApplicationException($"Ya existe un asiento con fila {request.Row} y número {request.SeatNumber} en esta sala");
                    }
                }

                var seat = _mapper.Map<Seat>(request);
                await _unitOfWork.Seats.AddAsync(seat);
                await _unitOfWork.CompleteAsync();

                var result = _mapper.Map<SeatDto>(seat);
                result.IsAvailable = true;

                return result;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al crear asiento: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al crear asiento: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<SeatDto> UpdateSeatAsync(int seatId, SeatRequest request)
        {
            try
            {
                var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
                if (seat == null)
                    throw new ApplicationException($"Asiento con ID {seatId} no encontrado");

                var hall = await _unitOfWork.CinemaHalls.GetByIdAsync(request.CinemaHallId);
                if (hall == null)
                    throw new ApplicationException($"Sala con ID {request.CinemaHallId} no encontrada");

                // Verificar que el asiento pertenezca a la sala especificada
                if (seat.CinemaHallId != request.CinemaHallId)
                    throw new ApplicationException("El asiento no pertenece a la sala especificada");

                // Verificar si la nueva posición del asiento ya está ocupada por otro asiento
                if (seat.Row != request.Row || seat.SeatNumber != request.SeatNumber)
                {
                    var existingSeats = await _unitOfWork.Seats.GetSeatsByCinemaHallAsync(request.CinemaHallId);
                    foreach (var existingSeat in existingSeats)
                    {
                        if (existingSeat.Id != seatId &&
                            existingSeat.Row.Equals(request.Row, StringComparison.OrdinalIgnoreCase) &&
                            existingSeat.SeatNumber.Equals(request.SeatNumber, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new ApplicationException($"Ya existe un asiento con fila {request.Row} y número {request.SeatNumber} en esta sala");
                        }
                    }
                }

                // Actualizar propiedades
                seat.Row = request.Row;
                seat.SeatNumber = request.SeatNumber;
                seat.IsActive = request.IsActive;

                await _unitOfWork.Seats.UpdateAsync(seat);
                await _unitOfWork.CompleteAsync();

                var result = _mapper.Map<SeatDto>(seat);
                result.IsAvailable = true;

                return result;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al actualizar asiento {seatId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al actualizar asiento {seatId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteSeatAsync(int seatId)
        {
            try
            {
                var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
                if (seat == null)
                    throw new ApplicationException($"Asiento con ID {seatId} no encontrado");

                // Verificar si el asiento está siendo utilizado en algún ticket
                var tickets = await _unitOfWork.Tickets.FindAsync(t => t.SeatId == seatId);
                if (tickets.Any())
                    throw new ApplicationException("No se puede eliminar un asiento que está siendo utilizado en tickets");

                // En lugar de eliminar el asiento, marcarlo como inactivo
                seat.IsActive = false;
                await _unitOfWork.Seats.UpdateAsync(seat);
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al eliminar asiento {seatId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al eliminar asiento {seatId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<bool> IsSeatAvailableForScreeningAsync(int seatId, int screeningId)
        {
            try
            {
                return await _unitOfWork.Seats.IsSeatAvailableForScreeningAsync(seatId, screeningId);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al verificar disponibilidad del asiento {seatId} para proyección {screeningId}: {ex.Message}", ex);
                throw;
            }
        }
    }
}