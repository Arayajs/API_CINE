using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using API_CINE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_CINE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IMovieScreeningService _screeningService;
        private readonly ILoggingService _loggingService;

        public MoviesController(
            IMovieService movieService,
            IMovieScreeningService screeningService,
            ILoggingService loggingService)
        {
            _movieService = movieService;
            _screeningService = screeningService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Obtiene todas las películas
        /// </summary>
        /// <returns>Lista de todas las películas</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IEnumerable<MovieDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetAllMovies()
        {
            try
            {
                var movies = await _movieService.GetAllMoviesAsync();
                return Ok(new ApiResponse<IEnumerable<MovieDto>>
                {
                    Success = true,
                    Message = "Películas obtenidas con éxito",
                    Data = movies
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener películas: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener las películas"
                });
            }
        }

        /// <summary>
        /// Obtiene las películas activas
        /// </summary>
        /// <returns>Lista de películas activas</returns>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IEnumerable<MovieDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetActiveMovies()
        {
            try
            {
                var movies = await _movieService.GetActiveMoviesAsync();
                return Ok(new ApiResponse<IEnumerable<MovieDto>>
                {
                    Success = true,
                    Message = "Películas activas obtenidas con éxito",
                    Data = movies
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener películas activas: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener las películas activas"
                });
            }
        }

        /// <summary>
        /// Obtiene una película por su ID
        /// </summary>
        /// <param name="id">ID de la película</param>
        /// <returns>Película solicitada</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<MovieDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetMovie(int id)
        {
            try
            {
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Película con ID {id} no encontrada"
                    });
                }

                return Ok(new ApiResponse<MovieDto>
                {
                    Success = true,
                    Message = "Película obtenida con éxito",
                    Data = movie
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener película con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener la película"
                });
            }
        }

        /// <summary>
        /// Obtiene las proyecciones para una película
        /// </summary>
        /// <param name="id">ID de la película</param>
        /// <returns>Lista de proyecciones de la película</returns>
        [HttpGet("{id}/screenings")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IEnumerable<MovieScreeningDto>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetMovieScreenings(int id)
        {
            try
            {
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Película con ID {id} no encontrada"
                    });
                }

                var screenings = await _screeningService.GetScreeningsByMovieAsync(id);
                return Ok(new ApiResponse<IEnumerable<MovieScreeningDto>>
                {
                    Success = true,
                    Message = "Proyecciones obtenidas con éxito",
                    Data = screenings
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener proyecciones para película con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener las proyecciones de la película"
                });
            }
        }

        /// <summary>
        /// Crea una nueva película
        /// </summary>
        /// <param name="movieRequest">Datos de la película</param>
        /// <returns>Película creada</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<MovieDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> CreateMovie([FromBody] MovieRequest movieRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Datos de película inválidos",
                        Data = ModelState
                    });
                }

                var movie = await _movieService.CreateMovieAsync(movieRequest);

                // Registrar la acción en la auditoría
                string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("Create", "Movie", movie.Id, userId,
                    $"Película creada: {movie.Title}", HttpContext.Connection.RemoteIpAddress?.ToString());

                return Created($"/api/movies/{movie.Id}", new ApiResponse<MovieDto>
                {
                    Success = true,
                    Message = "Película creada con éxito",
                    Data = movie
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al crear película: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al crear la película"
                });
            }
        }

        /// <summary>
        /// Actualiza una película existente
        /// </summary>
        /// <param name="id">ID de la película</param>
        /// <param name="movieRequest">Datos actualizados de la película</param>
        /// <returns>Película actualizada</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<MovieDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> UpdateMovie(int id, [FromBody] MovieRequest movieRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Datos de película inválidos",
                        Data = ModelState
                    });
                }

                var existingMovie = await _movieService.GetMovieByIdAsync(id);
                if (existingMovie == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Película con ID {id} no encontrada"
                    });
                }

                var movie = await _movieService.UpdateMovieAsync(id, movieRequest);

                // Registrar la acción en la auditoría
                string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("Update", "Movie", movie.Id, userId,
                    $"Película actualizada: {movie.Title}", HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(new ApiResponse<MovieDto>
                {
                    Success = true,
                    Message = "Película actualizada con éxito",
                    Data = movie
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al actualizar película con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al actualizar la película"
                });
            }
        }

        /// <summary>
        /// Elimina una película
        /// </summary>
        /// <param name="id">ID de la película</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            try
            {
                var existingMovie = await _movieService.GetMovieByIdAsync(id);
                if (existingMovie == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Película con ID {id} no encontrada"
                    });
                }

                var result = await _movieService.DeleteMovieAsync(id);

                // Registrar la acción en la auditoría
                string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("Delete", "Movie", id, userId,
                    $"Película eliminada: {existingMovie.Title}", HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Película eliminada con éxito",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al eliminar película con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al eliminar la película"
                });
            }
        }
    }
}