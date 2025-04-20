using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using API_CINE.Repositories.Interfaces;
using API_CINE.Services.Interfaces;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;



namespace API_CINE.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILoggingService _loggingService;

        public AuthService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IConfiguration configuration,
            ILoggingService loggingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _loggingService = loggingService;
        }

        public async Task<AuthResponse> RegisterUserAsync(RegisterRequest registerDto)
        {
            try
            {
                // Verificar si el correo ya existe
                var emailExists = await _unitOfWork.Users.CheckEmailExistsAsync(registerDto.Email);
                if (emailExists)
                {
                    throw new ApplicationException("El correo electrónico ya está registrado");
                }

                // Crear el usuario
                var user = new User
                {
                    Name = registerDto.Name,
                    Email = registerDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.CompleteAsync();

                // Asignar el rol de Customer por defecto
                var customerRole = await _unitOfWork.Roles.GetByNameAsync("Customer");
                if (customerRole == null)
                {
                    throw new ApplicationException("Rol de Customer no encontrado");
                }

                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = customerRole.Id
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.CompleteAsync();

                // Generar el token JWT
                var token = await GenerateJwtTokenAsync(user);

                // Mapear la respuesta
                var response = _mapper.Map<AuthResponse>(user);
                response.Roles = new List<string> { "Customer" };
                response.Token = token;
                response.TokenExpiration = DateTime.UtcNow.AddHours(24);

                _loggingService.LogInformation($"Usuario registrado con éxito: {user.Email}");

                return response;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al registrar usuario: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest loginDto)
        {
            try
            {
                // Buscar el usuario por email
                var user = await _unitOfWork.Users.GetByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    throw new ApplicationException("Correo electrónico o contraseña incorrectos");
                }

                // Verificar la contraseña
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
                if (!isPasswordValid)
                {
                    throw new ApplicationException("Correo electrónico o contraseña incorrectos");
                }

                // Obtener los roles del usuario
                var userWithRoles = await _unitOfWork.Users.GetUserWithRolesAsync(user.Id);
                var roles = userWithRoles.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList();

                // Generar el token JWT
                var token = await GenerateJwtTokenAsync(user);



                // Mapear la respuesta
                var response = _mapper.Map<AuthResponse>(user);
                response.Roles = roles;
                response.Token = token;
                response.TokenExpiration = DateTime.UtcNow.AddHours(24);

                _loggingService.LogInformation($"Usuario inició sesión con éxito: {user.Email}");

                return response;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al iniciar sesión: {ex.Message}", ex);
                throw;
            }
        }

        public static string GenerateSecureKey(int keySizeInBytes = 32)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] keyBytes = new byte[keySizeInBytes];
                rng.GetBytes(keyBytes);
                return Convert.ToBase64String(keyBytes);
            }
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            try
            {
                // Obtener los roles del usuario
                var userWithRoles = await _unitOfWork.Users.GetUserWithRolesAsync(user.Id);
                var roles = userWithRoles.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList();

                // Crear los claims
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
        };

                // Agregar los roles como claims
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // Configurar el token
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.UtcNow.AddHours(24);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: expires,
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al generar JWT token: {ex.Message}", ex);
                throw;
            }
        }

        public int? GetUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

                return userId;
            }
            catch
            {
                return null;
            }
        }
    }
}