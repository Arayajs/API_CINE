using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using API_CINE.Repositories.Interfaces;
using API_CINE.Services.Interfaces;
using AutoMapper;

namespace API_CINE.Services.Implementations
{
    public class MovieScreeningService : IMovieScreeningService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;

        public MovieScreeningService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggingService loggingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _loggingService = loggingService;
        }

        public async Task<MovieScreeningDto> GetScreeningByIdAsync(int screeningId)
        {
            try
            {
                var screening = await _unitOfWork.MovieScreenings.GetScreeningWithDetailsAsync(screeningId);
                if (screening == null)
                    return null;

                var result = _mapper.Map<MovieScreeningDto>(screening);

                // Calcular asientos disponibles
                var availableSeats = await _unitOfWork.Seats.GetAvailableSeatsForScreeningAsync(screeningId);
                result.AvailableSeats = availableSeats.Count();

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener proyección por ID {screeningId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<MovieScreeningDto>> GetAllScreeningsAsync()
        {
            try
            {
                var screenings = await _unitOfWork.MovieScreenings.GetAllAsync();
                var screeningDtos = _mapper.Map<IEnumerable<MovieScreeningDto>>(screenings);

                // Calcular asientos disponibles para cada función
                foreach (var screening in screeningDtos.ToList())
                {
                    var availableSeats = await _unitOfWork.Seats.GetAvailableSeatsForScreeningAsync(screening.Id);
                    screening.AvailableSeats = availableSeats.Count();
                }

                return screeningDtos;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener todas las proyecciones: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<MovieScreeningDto>> GetScreeningsByMovieAsync(int movieId)
        {
            try
            {
                var movie = await _unitOfWork.Movies.GetByIdAsync(movieId);
                if (movie == null)
                    throw new ApplicationException($"Película con ID {movieId} no encontrada");

                var screenings = await _unitOfWork.MovieScreenings.GetScreeningsByMovieAsync(movieId);
                var screeningDtos = _mapper.Map<IEnumerable<MovieScreeningDto>>(screenings);

                // Calcular asientos disponibles para cada función
                foreach (var screening in screeningDtos.ToList())
                {
                    var availableSeats = await _unitOfWork.Seats.GetAvailableSeatsForScreeningAsync(screening.Id);
                    screening.AvailableSeats = availableSeats.Count();
                }

                return screeningDtos;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al obtener proyecciones para película {movieId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener proyecciones para película {movieId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<MovieScreeningDto>> GetScreeningsByCinemaAsync(int cinemaId)
        {
            try
            {
                var cinema = await _unitOfWork.Cinemas.GetByIdAsync(cinemaId);
                if (cinema == null)
                    throw new ApplicationException($"Cine con ID {cinemaId} no encontrado");

                var screenings = await _unitOfWork.MovieScreenings.GetScreeningsByCinemaAsync(cinemaId);
                var screeningDtos = _mapper.Map<IEnumerable<MovieScreeningDto>>(screenings);

                // Calcular asientos disponibles para cada función
                foreach (var screening in screeningDtos.ToList())
                {
                    var availableSeats = await _unitOfWork.Seats.GetAvailableSeatsForScreeningAsync(screening.Id);
                    screening.AvailableSeats = availableSeats.Count();
                }

                return screeningDtos;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al obtener proyecciones para cine {cinemaId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener proyecciones para cine {cinemaId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<MovieScreeningDto>> GetScreeningsByCinemaHallAsync(int hallId)
        {
            try
            {
                var hall = await _unitOfWork.CinemaHalls.GetByIdAsync(hallId);
                if (hall == null)
                    throw new ApplicationException($"Sala con ID {hallId} no encontrada");

                var screenings = await _unitOfWork.MovieScreenings.GetScreeningsByCinemaHallAsync(hallId);
                var screeningDtos = _mapper.Map<IEnumerable<MovieScreeningDto>>(screenings);

                // Calcular asientos disponibles para cada función
                foreach (var screening in screeningDtos.ToList())
                {
                    var availableSeats = await _unitOfWork.Seats.GetAvailableSeatsForScreeningAsync(screening.Id);
                    screening.AvailableSeats = availableSeats.Count();
                }

                return screeningDtos;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al obtener proyecciones para sala {hallId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener proyecciones para sala {hallId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<MovieScreeningDto>> GetScreeningsByDateAsync(DateTime date)
        {
            try
            {
                var screenings = await _unitOfWork.MovieScreenings.GetScreeningsByDateAsync(date);
                var screeningDtos = _mapper.Map<IEnumerable<MovieScreeningDto>>(screenings);

                // Calcular asientos disponibles para cada función
                foreach (var screening in screeningDtos.ToList())
                {
                    var availableSeats = await _unitOfWork.Seats.GetAvailableSeatsForScreeningAsync(screening.Id);
                    screening.AvailableSeats = availableSeats.Count();
                }

                return screeningDtos;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener proyecciones para fecha {date}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<MovieScreeningDto> CreateScreeningAsync(MovieScreeningRequest request)
        {
            try
            {
                // Validar que la película exista
                var movie = await _unitOfWork.Movies.GetByIdAsync(request.MovieId);
                if (movie == null)
                    throw new ApplicationException($"Película con ID {request.MovieId} no encontrada");

                // Validar que la sala exista
                var hall = await _unitOfWork.CinemaHalls.GetByIdAsync(request.CinemaHallId);
                if (hall == null)
                    throw new ApplicationException($"Sala con ID {request.CinemaHallId} no encontrada");

                // Validar que la película y la sala estén activas
                if (!movie.IsActive)
                    throw new ApplicationException("No se puede crear una proyección con una película inactiva");

                if (!hall.IsActive)
                    throw new ApplicationException("No se puede crear una proyección en una sala inactiva");

                // Validar que la hora de inicio sea después de la hora actual
                if (request.StartTime <= DateTime.Now)
                    throw new ApplicationException("La hora de inicio debe ser posterior a la hora actual");

                // Validar que la hora de finalización sea después de la hora de inicio
                if (request.EndTime <= request.StartTime)
                    throw new ApplicationException("La hora de finalización debe ser posterior a la hora de inicio");

                // Validar que la duración coincida aproximadamente con la duración de la película
                TimeSpan scheduledDuration = request.EndTime - request.StartTime;
                // Permitir una diferencia de hasta 15 minutos para incluir anuncios, etc.
                if (Math.Abs((scheduledDuration - movie.Duration).TotalMinutes) > 15)
                    throw new ApplicationException("La duración programada debe coincidir aproximadamente con la duración de la película");

                // Validar que no haya solapamiento con otras proyecciones en la misma sala
                var hallScreenings = await _unitOfWork.MovieScreenings.GetScreeningsByCinemaHallAsync(request.CinemaHallId);
                foreach (var existingScreening in hallScreenings)
                {
                    if (existingScreening.IsActive &&
                        ((request.StartTime >= existingScreening.StartTime && request.StartTime < existingScreening.EndTime) ||
                         (request.EndTime > existingScreening.StartTime && request.EndTime <= existingScreening.EndTime) ||
                         (request.StartTime <= existingScreening.StartTime && request.EndTime >= existingScreening.EndTime)))
                    {
                        throw new ApplicationException("La proyección se solapa con otra proyección en la misma sala");
                    }
                }

                // Crear la proyección
                var screening = new MovieScreening
                {
                    MovieId = request.MovieId,
                    CinemaHallId = request.CinemaHallId,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    TicketPrice = request.TicketPrice,
                    IsActive = request.IsActive
                };

                await _unitOfWork.MovieScreenings.AddAsync(screening);
                await _unitOfWork.CompleteAsync();

                // Obtener la proyección creada con todos los detalles
                var createdScreening = await _unitOfWork.MovieScreenings.GetScreeningWithDetailsAsync(screening.Id);
                var result = _mapper.Map<MovieScreeningDto>(createdScreening);

                // Calcular asientos disponibles
                var availableSeats = await _unitOfWork.Seats.GetAvailableSeatsForScreeningAsync(screening.Id);
                result.AvailableSeats = availableSeats.Count();

                return result;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al crear proyección: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al crear proyección: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<MovieScreeningDto> UpdateScreeningAsync(int screeningId, MovieScreeningRequest request)
        {
            try
            {
                var screening = await _unitOfWork.MovieScreenings.GetScreeningWithDetailsAsync(screeningId);
                if (screening == null)
                    throw new ApplicationException($"Proyección con ID {screeningId} no encontrada");

                // Verificar si hay tickets vendidos
                var tickets = await _unitOfWork.Tickets.GetTicketsByScreeningAsync(screeningId);
                if (tickets.Any())
                    throw new ApplicationException("No se puede modificar una proyección con tickets vendidos");

                // Validar que la película exista
                var movie = await _unitOfWork.Movies.GetByIdAsync(request.MovieId);
                if (movie == null)
                    throw new ApplicationException($"Película con ID {request.MovieId} no encontrada");

                // Validar que la sala exista
                var hall = await _unitOfWork.CinemaHalls.GetByIdAsync(request.CinemaHallId);
                if (hall == null)
                    throw new ApplicationException($"Sala con ID {request.CinemaHallId} no encontrada");

                // Validar que la película y la sala estén activas
                if (!movie.IsActive)
                    throw new ApplicationException("No se puede asignar una película inactiva a una proyección");

                if (!hall.IsActive)
                    throw new ApplicationException("No se puede asignar una sala inactiva a una proyección");

                // Validar que la hora de inicio sea después de la hora actual
                if (request.StartTime <= DateTime.Now)
                    throw new ApplicationException("La hora de inicio debe ser posterior a la hora actual");

                // Validar que la hora de finalización sea después de la hora de inicio
                if (request.EndTime <= request.StartTime)
                    throw new ApplicationException("La hora de finalización debe ser posterior a la hora de inicio");

                // Validar que la duración coincida aproximadamente con la duración de la película
                TimeSpan scheduledDuration = request.EndTime - request.StartTime;
                if (Math.Abs((scheduledDuration - movie.Duration).TotalMinutes) > 15)
                    throw new ApplicationException("La duración programada debe coincidir aproximadamente con la duración de la película");

                // Validar que no haya solapamiento con otras proyecciones en la misma sala
                var hallScreenings = await _unitOfWork.MovieScreenings.GetScreeningsByCinemaHallAsync(request.CinemaHallId);
                foreach (var otherScreening in hallScreenings)
                {
                    if (otherScreening.Id != screeningId && otherScreening.IsActive &&
                        ((request.StartTime >= otherScreening.StartTime && request.StartTime < otherScreening.EndTime) ||
                         (request.EndTime > otherScreening.StartTime && request.EndTime <= otherScreening.EndTime) ||
                         (request.StartTime <= otherScreening.StartTime && request.EndTime >= otherScreening.EndTime)))
                    {
                        throw new ApplicationException("La proyección se solapa con otra proyección en la misma sala");
                    }
                }

                // Actualizar la proyección
                screening.MovieId = request.MovieId;
                screening.CinemaHallId = request.CinemaHallId;
                screening.StartTime = request.StartTime;
                screening.EndTime = request.EndTime;
                screening.TicketPrice = request.TicketPrice;
                screening.IsActive = request.IsActive;

                await _unitOfWork.MovieScreenings.UpdateAsync(screening);
                await _unitOfWork.CompleteAsync();

                // Obtener la proyección actualizada con todos los detalles
                var updatedScreening = await _unitOfWork.MovieScreenings.GetScreeningWithDetailsAsync(screeningId);
                var result = _mapper.Map<MovieScreeningDto>(updatedScreening);

                // Calcular asientos disponibles
                var availableSeats = await _unitOfWork.Seats.GetAvailableSeatsForScreeningAsync(screeningId);
                result.AvailableSeats = availableSeats.Count();

                return result;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al actualizar proyección {screeningId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al actualizar proyección {screeningId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteScreeningAsync(int screeningId)
        {
            try
            {
                var screening = await _unitOfWork.MovieScreenings.GetScreeningWithDetailsAsync(screeningId);
                if (screening == null)
                    throw new ApplicationException($"Proyección con ID {screeningId} no encontrada");

                // Verificar si hay tickets vendidos
                var tickets = await _unitOfWork.Tickets.GetTicketsByScreeningAsync(screeningId);
                if (tickets.Any())
                    throw new ApplicationException("No se puede eliminar una proyección con tickets vendidos");

                // En lugar de eliminar la proyección, desactivarla
                screening.IsActive = false;
                await _unitOfWork.MovieScreenings.UpdateAsync(screening);
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al eliminar proyección {screeningId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al eliminar proyección {screeningId}: {ex.Message}", ex);
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

                // Marcar todos los asientos como disponibles
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
    }
}