using API_CINE.Models.DTOs;
using API_CINE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_CINE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILoggingService _loggingService;

        public AuthController(IAuthService authService, ILoggingService loggingService)
        {
            _authService = authService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        /// <param name="registerDto">Datos de registro del usuario</param>
        /// <returns>Datos del usuario registrado con token de autenticación</returns>
        [HttpPost("register")]

        public async Task<IActionResult> Register([FromBody] RegisterRequest registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Datos de registro inválidos",
                        Data = ModelState
                    });
                }

                var result = await _authService.RegisterUserAsync(registerDto);

                return Created("", new ApiResponse<AuthResponse>
                {
                    Success = true,
                    Message = "Usuario registrado con éxito",
                    Data = result
                });
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error en registro: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
           
        }

        /// <summary>
        /// Inicia sesión de un usuario en el sistema
        /// </summary>
        /// <param name="loginDto">Datos de inicio de sesión</param>
        /// <returns>Datos del usuario con token de autenticación</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<AuthResponse>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Datos de inicio de sesión inválidos",
                        Data = ModelState
                    });
                }

                var result = await _authService.LoginAsync(loginDto);

                return Ok(new ApiResponse<AuthResponse>
                {
                    Success = true,
                    Message = "Inicio de sesión exitoso",
                    Data = result
                });
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error en inicio de sesión: {ex.Message}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error en inicio de sesión: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al procesar la solicitud de inicio de sesión"
                });
            }
        }
    }
}