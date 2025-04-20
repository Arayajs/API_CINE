using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using API_CINE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_CINE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CinemasController : ControllerBase
    {
        private readonly ICinemaService _cinemaService;
        private readonly ICinemaHallService _cinemaHallService;
        private readonly IMovieService _movieService;
        private readonly IMovieScreeningService _screeningService;
        private readonly ILoggingService _loggingService;

        public CinemasController(
            ICinemaService cinemaService,
            ICinemaHallService cinemaHallService,
            IMovieService movieService,
            IMovieScreeningService screeningService,
            ILoggingService loggingService)
        {
            _cinemaService = cinemaService;
            _cinemaHallService = cinemaHallService;
            _movieService = movieService;
            _screeningService = screeningService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Obtiene todos los cines
        /// </summary>
        /// <returns>Lista de todos los cines</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IEnumerable<CinemaDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetAllCinemas()
        {
            var cinemas = await _cinemaService.GetAllCinemasAsync();
            return Ok(new ApiResponse<IEnumerable<CinemaDto>>
            {
                Success = true,
                Message = "Cines obtenidos con éxito",
                Data = cinemas
            });
        }

        /// <summary>
        /// Obtiene los cines activos
        /// </summary>
        /// <returns>Lista de cines activos</returns>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IEnumerable<CinemaDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetActiveCinemas()
        {
            try
            {
                var cinemas = await _cinemaService.GetActiveCinemasAsync();
                return Ok(new ApiResponse<IEnumerable<CinemaDto>>
                {
                    Success = true,
                    Message = "Cines activos obtenidos con éxito",
                    Data = cinemas
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener cines activos: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener los cines activos"
                });
            }
        }

        /// <summary>
        /// Obtiene un cine por su ID
        /// </summary>
        /// <param name="id">ID del cine</param>
        /// <returns>Cine solicitado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<CinemaDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetCinema(int id)
        {
            try
            {
                var cinema = await _cinemaService.GetCinemaByIdAsync(id);
                if (cinema == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Cine con ID {id} no encontrado"
                    });
                }

                return Ok(new ApiResponse<CinemaDto>
                {
                    Success = true,
                    Message = "Cine obtenido con éxito",
                    Data = cinema
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener cine con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener el cine"
                });
            }
        }

        /// <summary>
        /// Obtiene las salas de un cine
        /// </summary>
        /// <param name="id">ID del cine</param>
        /// <returns>Lista de salas del cine</returns>
        [HttpGet("{id}/halls")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IEnumerable<CinemaHallDto>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetCinemaHalls(int id)
        {
            try
            {
                var cinema = await _cinemaService.GetCinemaByIdAsync(id);
                if (cinema == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Cine con ID {id} no encontrado"
                    });
                }

                var halls = await _cinemaHallService.GetHallsByCinemaAsync(id);
                return Ok(new ApiResponse<IEnumerable<CinemaHallDto>>
                {
                    Success = true,
                    Message = "Salas obtenidas con éxito",
                    Data = halls
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener salas para cine con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener las salas del cine"
                });
            }
        }

        /// <summary>
        /// Obtiene las películas que se proyectan en un cine
        /// </summary>
        /// <param name="id">ID del cine</param>
        /// <returns>Lista de películas del cine</returns>
        [HttpGet("{id}/movies")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IEnumerable<MovieDto>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetCinemaMovies(int id)
        {
            try
            {
                var cinema = await _cinemaService.GetCinemaByIdAsync(id);
                if (cinema == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Cine con ID {id} no encontrado"
                    });
                }

                var movies = await _cinemaService.GetMoviesByCinemaAsync(id);
                return Ok(new ApiResponse<IEnumerable<MovieDto>>
                {
                    Success = true,
                    Message = "Películas obtenidas con éxito",
                    Data = movies
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener películas para cine con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener las películas del cine"
                });
            }
        }

        /// <summary>
        /// Obtiene las proyecciones de un cine
        /// </summary>
        /// <param name="id">ID del cine</param>
        /// <returns>Lista de proyecciones del cine</returns>
        [HttpGet("{id}/screenings")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IEnumerable<MovieScreeningDto>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetCinemaScreenings(int id)
        {
            try
            {
                var cinema = await _cinemaService.GetCinemaByIdAsync(id);
                if (cinema == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Cine con ID {id} no encontrado"
                    });
                }

                var screenings = await _screeningService.GetScreeningsByCinemaAsync(id);
                return Ok(new ApiResponse<IEnumerable<MovieScreeningDto>>
                {
                    Success = true,
                    Message = "Proyecciones obtenidas con éxito",
                    Data = screenings
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener proyecciones para cine con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener las proyecciones del cine"
                });
            }
        }

        /// <summary>
        /// Crea un nuevo cine
        /// </summary>
        /// <param name="cinemaRequest">Datos del cine</param>
        /// <returns>Cine creado</returns>
        [HttpPost]
       
        public async Task<IActionResult> CreateCinema([FromBody] CinemaRequest cinemaRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Datos de cine inválidos",
                        Data = ModelState
                    });
                }

                var cinema = await _cinemaService.CreateCinemaAsync(cinemaRequest);

                // Registrar la acción en la auditoría
                string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("Create", "Cinema", cinema.Id, userId,
                    $"Cine creado: {cinema.Name}", HttpContext.Connection.RemoteIpAddress?.ToString());

                return Created($"/api/cinemas/{cinema.Id}", new ApiResponse<CinemaDto>
                {
                    Success = true,
                    Message = "Cine creado con éxito",
                    Data = cinema
                });
            }
            
        }

        /// <summary>
        /// Actualiza un cine existente
        /// </summary>
        /// <param name="id">ID del cine</param>
        /// <param name="cinemaRequest">Datos actualizados del cine</param>
        /// <returns>Cine actualizado</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<CinemaDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> UpdateCinema(int id, [FromBody] CinemaRequest cinemaRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Datos de cine inválidos",
                        Data = ModelState
                    });
                }

                var existingCinema = await _cinemaService.GetCinemaByIdAsync(id);
                if (existingCinema == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Cine con ID {id} no encontrado"
                    });
                }

                var cinema = await _cinemaService.UpdateCinemaAsync(id, cinemaRequest);

                // Registrar la acción en la auditoría
                string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("Update", "Cinema", cinema.Id, userId,
                    $"Cine actualizado: {cinema.Name}", HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(new ApiResponse<CinemaDto>
                {
                    Success = true,
                    Message = "Cine actualizado con éxito",
                    Data = cinema
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al actualizar cine con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al actualizar el cine"
                });
            }
        }

        /// <summary>
        /// Elimina un cine
        /// </summary>
        /// <param name="id">ID del cine</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> DeleteCinema(int id)
        {
            try
            {
                var existingCinema = await _cinemaService.GetCinemaByIdAsync(id);
                if (existingCinema == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Cine con ID {id} no encontrado"
                    });
                }

                var result = await _cinemaService.DeleteCinemaAsync(id);

                // Registrar la acción en la auditoría
                string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("Delete", "Cinema", id, userId,
                    $"Cine eliminado: {existingCinema.Name}", HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Cine eliminado con éxito",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al eliminar cine con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al eliminar el cine"
                });
            }
        }

        /// <summary>
        /// Crea una nueva sala en un cine
        /// </summary>
        /// <param name="id">ID del cine</param>
        /// <param name="hallRequest">Datos de la sala</param>
        /// <returns>Sala creada</returns>
        [HttpPost("{id}/halls")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<CinemaHallDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> CreateCinemaHall(int id, [FromBody] CinemaHallRequest hallRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Datos de sala inválidos",
                        Data = ModelState
                    });
                }

                var cinema = await _cinemaService.GetCinemaByIdAsync(id);
                if (cinema == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Cine con ID {id} no encontrado"
                    });
                }

                // Asegurar que el ID de cine en la solicitud coincida con el ID de la ruta
                hallRequest.CinemaId = id;

                var hall = await _cinemaHallService.CreateCinemaHallAsync(hallRequest);

                // Registrar la acción en la auditoría
                string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("Create", "CinemaHall", hall.Id, userId,
                    $"Sala creada: {hall.Name} en cine {cinema.Name}", HttpContext.Connection.RemoteIpAddress?.ToString());

                return Created($"/api/cinemahalls/{hall.Id}", new ApiResponse<CinemaHallDto>
                {
                    Success = true,
                    Message = "Sala creada con éxito",
                    Data = hall
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al crear sala para cine con ID {id}: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al crear la sala"
                });
            }
        }
    }
}