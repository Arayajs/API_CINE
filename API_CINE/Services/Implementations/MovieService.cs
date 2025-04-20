using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using API_CINE.Repositories.Interfaces;
using API_CINE.Services.Interfaces;
using AutoMapper;

namespace API_CINE.Services.Implementations
{
    public class MovieService : IMovieService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;

        public MovieService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggingService loggingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _loggingService = loggingService;
        }

        public async Task<MovieDto> GetMovieByIdAsync(int movieId)
        {
            try
            {
                var movie = await _unitOfWork.Movies.GetMovieWithScreeningsAsync(movieId);
                return _mapper.Map<MovieDto>(movie);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener película por ID {movieId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<MovieDto>> GetAllMoviesAsync()
        {
            try
            {
                var movies = await _unitOfWork.Movies.GetAllAsync();
                return _mapper.Map<IEnumerable<MovieDto>>(movies);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener todas las películas: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<MovieDto>> GetActiveMoviesAsync()
        {
            try
            {
                var movies = await _unitOfWork.Movies.GetActiveMoviesAsync();
                return _mapper.Map<IEnumerable<MovieDto>>(movies);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener películas activas: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<MovieDto> CreateMovieAsync(MovieRequest request)
        {
            try
            {
                // Validar duración
                if (request.Duration.TotalMinutes <= 0)
                    throw new ApplicationException("La duración de la película debe ser mayor a 0 minutos");

                var movie = _mapper.Map<Movie>(request);
                await _unitOfWork.Movies.AddAsync(movie);
                await _unitOfWork.CompleteAsync();

                return _mapper.Map<MovieDto>(movie);
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al crear película: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al crear película: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<MovieDto> UpdateMovieAsync(int movieId, MovieRequest request)
        {
            try
            {
                var movie = await _unitOfWork.Movies.GetByIdAsync(movieId);
                if (movie == null)
                    throw new ApplicationException($"Película con ID {movieId} no encontrada");

                // Validar duración
                if (request.Duration.TotalMinutes <= 0)
                    throw new ApplicationException("La duración de la película debe ser mayor a 0 minutos");

                // Actualizar propiedades
                movie.Title = request.Title;
                movie.Description = request.Description;
                movie.Duration = request.Duration;
                movie.Genre = request.Genre;
                movie.Director = request.Director;
                movie.ImageUrl = request.ImageUrl;
                movie.ReleaseDate = request.ReleaseDate;
                movie.Rating = request.Rating;
                movie.IsActive = request.IsActive;

                await _unitOfWork.Movies.UpdateAsync(movie);
                await _unitOfWork.CompleteAsync();

                return _mapper.Map<MovieDto>(movie);
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al actualizar película {movieId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al actualizar película {movieId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteMovieAsync(int movieId)
        {
            try
            {
                var movie = await _unitOfWork.Movies.GetMovieWithScreeningsAsync(movieId);
                if (movie == null)
                    throw new ApplicationException($"Película con ID {movieId} no encontrada");

                // Verificar si hay proyecciones futuras
                if (movie.MovieScreenings.Any(s => s.StartTime > DateTime.Now && s.IsActive))
                    throw new ApplicationException("No se puede eliminar una película con proyecciones futuras programadas");

                // En lugar de eliminar físicamente, marcar como inactiva
                movie.IsActive = false;

                // También marcar proyecciones futuras como inactivas
                foreach (var screening in movie.MovieScreenings.Where(s => s.StartTime > DateTime.Now))
                {
                    screening.IsActive = false;
                    await _unitOfWork.MovieScreenings.UpdateAsync(screening);
                }

                await _unitOfWork.Movies.UpdateAsync(movie);
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al eliminar película {movieId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al eliminar película {movieId}: {ex.Message}", ex);
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
    }
}