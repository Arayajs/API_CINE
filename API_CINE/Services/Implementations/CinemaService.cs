using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using API_CINE.Repositories.Interfaces;
using API_CINE.Services.Interfaces;
using AutoMapper;

namespace API_CINE.Services.Implementations
{
    public class CinemaService : ICinemaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;

        public CinemaService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggingService loggingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _loggingService = loggingService;
        }

        public async Task<CinemaDto> GetCinemaByIdAsync(int cinemaId)
        {
            try
            {
                var cinema = await _unitOfWork.Cinemas.GetCinemaWithHallsAsync(cinemaId);
                return _mapper.Map<CinemaDto>(cinema);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener cine por ID {cinemaId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<CinemaDto>> GetAllCinemasAsync()
        {
            try
            {
                var cinemas = await _unitOfWork.Cinemas.GetAllAsync();
                return _mapper.Map<IEnumerable<CinemaDto>>(cinemas);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener todos los cines: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<CinemaDto>> GetActiveCinemasAsync()
        {
            try
            {
                var cinemas = await _unitOfWork.Cinemas.GetActiveCinemasAsync();
                return _mapper.Map<IEnumerable<CinemaDto>>(cinemas);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener cines activos: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<CinemaDto> CreateCinemaAsync(CinemaRequest request)
        {
            try
            {
                var cinema = _mapper.Map<Cinema>(request);
                await _unitOfWork.Cinemas.AddAsync(cinema);
                await _unitOfWork.CompleteAsync();

                return _mapper.Map<CinemaDto>(cinema);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al crear cine: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<CinemaDto> UpdateCinemaAsync(int cinemaId, CinemaRequest request)
        {
            try
            {
                var cinema = await _unitOfWork.Cinemas.GetByIdAsync(cinemaId);
                if (cinema == null)
                    throw new ApplicationException($"Cine con ID {cinemaId} no encontrado");

                // Actualizar propiedades
                cinema.Name = request.Name;
                cinema.Address = request.Address;
                cinema.Description = request.Description;
                cinema.City = request.City;
                cinema.IsActive = request.IsActive;

                await _unitOfWork.Cinemas.UpdateAsync(cinema);
                await _unitOfWork.CompleteAsync();

                // Obtener cine actualizado con salas
                var updatedCinema = await _unitOfWork.Cinemas.GetCinemaWithHallsAsync(cinemaId);
                return _mapper.Map<CinemaDto>(updatedCinema);
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al actualizar cine {cinemaId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al actualizar cine {cinemaId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteCinemaAsync(int cinemaId)
        {
            try
            {
                var cinema = await _unitOfWork.Cinemas.GetCinemaWithHallsAsync(cinemaId);
                if (cinema == null)
                    throw new ApplicationException($"Cine con ID {cinemaId} no encontrado");

                // Verificar si hay proyecciones activas
                foreach (var hall in cinema.CinemaHalls)
                {
                    var screenings = await _unitOfWork.MovieScreenings.GetScreeningsByCinemaHallAsync(hall.Id);
                    if (screenings.Any(s => s.StartTime > DateTime.Now))
                        throw new ApplicationException("No se puede eliminar un cine con proyecciones futuras programadas");
                }

                // Establecer como inactivo en lugar de eliminar
                cinema.IsActive = false;
                await _unitOfWork.Cinemas.UpdateAsync(cinema);
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al eliminar cine {cinemaId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al eliminar cine {cinemaId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<MovieDto>> GetMoviesByCinemaAsync(int cinemaId)
        {
            try
            {
                var cinema = await _unitOfWork.Cinemas.GetByIdAsync(cinemaId);
                if (cinema == null)
                    throw new ApplicationException($"Cine con ID {cinemaId} no encontrado");

                var movies = await _unitOfWork.Movies.GetMoviesByCinemaAsync(cinemaId);
                return _mapper.Map<IEnumerable<MovieDto>>(movies);
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al obtener películas por cine {cinemaId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener películas por cine {cinemaId}: {ex.Message}", ex);
                throw;
            }
        }
    }
}