using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using API_CINE.Repositories.Interfaces;
using API_CINE.Services.Interfaces;
using AutoMapper;

namespace API_CINE.Services.Implementations
{
    public class CinemaHallService : ICinemaHallService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;

        public CinemaHallService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggingService loggingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _loggingService = loggingService;
        }

        public async Task<CinemaHallDto> GetCinemaHallByIdAsync(int hallId)
        {
            try
            {
                var hall = await _unitOfWork.CinemaHalls.GetHallWithSeatsAsync(hallId);
                if (hall == null)
                    return null;

                var result = _mapper.Map<CinemaHallDto>(hall);

                // Obtener información del cine
                var cinema = await _unitOfWork.Cinemas.GetByIdAsync(hall.CinemaId);
                if (cinema != null)
                {
                    result.CinemaName = cinema.Name;
                }

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener sala por ID {hallId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<CinemaHallDto>> GetHallsByCinemaAsync(int cinemaId)
        {
            try
            {
                var cinema = await _unitOfWork.Cinemas.GetByIdAsync(cinemaId);
                if (cinema == null)
                    throw new ApplicationException($"Cine con ID {cinemaId} no encontrado");

                var halls = await _unitOfWork.CinemaHalls.GetHallsByCinemaAsync(cinemaId);
                var hallDtos = _mapper.Map<IEnumerable<CinemaHallDto>>(halls);

                // Asignar nombre del cine a cada sala
                foreach (var hall in hallDtos)
                {
                    hall.CinemaName = cinema.Name;
                }

                return hallDtos;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al obtener salas por cine {cinemaId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener salas por cine {cinemaId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<CinemaHallDto> CreateCinemaHallAsync(CinemaHallRequest request)
        {
            try
            {
                var cinema = await _unitOfWork.Cinemas.GetByIdAsync(request.CinemaId);
                if (cinema == null)
                    throw new ApplicationException($"Cine con ID {request.CinemaId} no encontrado");

                var hall = _mapper.Map<CinemaHall>(request);
                await _unitOfWork.CinemaHalls.AddAsync(hall);
                await _unitOfWork.CompleteAsync();

                // Crear asientos para la sala
                await CreateSeatsForHall(hall.Id, hall.Capacity);

                // Obtener la sala con los asientos
                var createdHall = await _unitOfWork.CinemaHalls.GetHallWithSeatsAsync(hall.Id);
                var result = _mapper.Map<CinemaHallDto>(createdHall);
                result.CinemaName = cinema.Name;

                return result;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al crear sala: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al crear sala: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<CinemaHallDto> UpdateCinemaHallAsync(int hallId, CinemaHallRequest request)
        {
            try
            {
                var hall = await _unitOfWork.CinemaHalls.GetHallWithSeatsAsync(hallId);
                if (hall == null)
                    throw new ApplicationException($"Sala con ID {hallId} no encontrada");

                var cinema = await _unitOfWork.Cinemas.GetByIdAsync(request.CinemaId);
                if (cinema == null)
                    throw new ApplicationException($"Cine con ID {request.CinemaId} no encontrado");

                // Verificar que la sala pertenezca al cine especificado
                if (hall.CinemaId != request.CinemaId)
                    throw new ApplicationException("La sala no pertenece al cine especificado");

                // Verificar si hay proyecciones activas
                var screenings = await _unitOfWork.MovieScreenings.GetScreeningsByCinemaHallAsync(hallId);
                if (screenings.Any(s => s.StartTime > DateTime.Now))
                    throw new ApplicationException("No se puede modificar una sala con proyecciones futuras programadas");

                // Actualizar propiedades
                hall.Name = request.Name;
                hall.HallType = request.HallType;

                // Si la capacidad ha cambiado, puede ser necesario ajustar los asientos
                if (hall.Capacity != request.Capacity)
                {
                    // Si la capacidad aumentó, crear nuevos asientos
                    if (request.Capacity > hall.Capacity)
                    {
                        await CreateSeatsForHall(hall.Id, request.Capacity - hall.Capacity);
                    }
                    // Si la capacidad disminuyó, marcar algunos asientos como inactivos
                    else if (request.Capacity < hall.Capacity)
                    {
                        await DeactivateExcessSeats(hall.Id, hall.Capacity - request.Capacity);
                    }

                    hall.Capacity = request.Capacity;
                }

                await _unitOfWork.CinemaHalls.UpdateAsync(hall);
                await _unitOfWork.CompleteAsync();

                // Obtener la sala actualizada
                var updatedHall = await _unitOfWork.CinemaHalls.GetHallWithSeatsAsync(hallId);
                var result = _mapper.Map<CinemaHallDto>(updatedHall);
                result.CinemaName = cinema.Name;

                return result;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al actualizar sala {hallId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al actualizar sala {hallId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteCinemaHallAsync(int hallId)
        {
            try
            {
                var hall = await _unitOfWork.CinemaHalls.GetHallWithScreeningsAsync(hallId);
                if (hall == null)
                    throw new ApplicationException($"Sala con ID {hallId} no encontrada");

                // Verificar si hay proyecciones activas
                if (hall.MovieScreenings.Any(s => s.StartTime > DateTime.Now))
                    throw new ApplicationException("No se puede eliminar una sala con proyecciones futuras programadas");

                // Marcar todos los asientos como inactivos
                var seats = await _unitOfWork.Seats.GetSeatsByCinemaHallAsync(hallId);
                foreach (var seat in seats)
                {
                    seat.IsActive = false;
                    await _unitOfWork.Seats.UpdateAsync(seat);
                }

                // Marcar todas las proyecciones como inactivas
                foreach (var screening in hall.MovieScreenings)
                {
                    screening.IsActive = false;
                    await _unitOfWork.MovieScreenings.UpdateAsync(screening);
                }

                // En lugar de eliminar la sala, marcarla como inactiva
                hall.Cinema.IsActive = false;
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al eliminar sala {hallId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al eliminar sala {hallId}: {ex.Message}", ex);
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
                return _mapper.Map<IEnumerable<SeatDto>>(seats);
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

        // Métodos privados para manejar asientos
        private async Task CreateSeatsForHall(int hallId, int numberOfSeats)
        {
            // Obtener todos los asientos actuales para saber por dónde empezar
            var existingSeats = await _unitOfWork.Seats.GetSeatsByCinemaHallAsync(hallId);
            int totalExistingSeats = existingSeats.Count();

            // Calcular filas y asientos por fila (por ejemplo, 10 asientos por fila)
            int seatsPerRow = 10;

            for (int i = 0; i < numberOfSeats; i++)
            {
                int currentSeatNumber = totalExistingSeats + i + 1;
                int rowNumber = (currentSeatNumber - 1) / seatsPerRow + 1;
                int seatNumber = (currentSeatNumber - 1) % seatsPerRow + 1;

                string rowLetter = GetRowLetter(rowNumber);

                var seat = new Seat
                {
                    CinemaHallId = hallId,
                    Row = rowLetter,
                    SeatNumber = seatNumber.ToString(),
                    IsActive = true
                };

                await _unitOfWork.Seats.AddAsync(seat);
            }

            await _unitOfWork.CompleteAsync();
        }

        private async Task DeactivateExcessSeats(int hallId, int numberOfSeatsToDeactivate)
        {
            // Obtener todos los asientos ordenados por ID (asumiendo que los más nuevos tienen ID mayor)
            var seats = (await _unitOfWork.Seats.GetSeatsByCinemaHallAsync(hallId))
                .OrderByDescending(s => s.Id)
                .Take(numberOfSeatsToDeactivate)
                .ToList();

            foreach (var seat in seats)
            {
                seat.IsActive = false;
                await _unitOfWork.Seats.UpdateAsync(seat);
            }

            await _unitOfWork.CompleteAsync();
        }

        private string GetRowLetter(int rowNumber)
        {
            // Convertir número de fila a letra (A-Z, AA-ZZ, etc.)
            string result = "";

            while (rowNumber > 0)
            {
                int remainder = (rowNumber - 1) % 26;
                char letter = (char)('A' + remainder);
                result = letter + result;
                rowNumber = (rowNumber - 1) / 26;
            }

            return result;
        }
    }
}