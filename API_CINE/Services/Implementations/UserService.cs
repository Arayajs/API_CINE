using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using API_CINE.Repositories.Interfaces;
using API_CINE.Services.Interfaces;
using AutoMapper;

namespace API_CINE.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;

        public UserService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggingService loggingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _loggingService = loggingService;
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetUserWithRolesAsync(userId);
                if (user == null)
                    return null;

                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

                return userDto;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener usuario por ID {userId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllAsync();
                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var userWithRoles = await _unitOfWork.Users.GetUserWithRolesAsync(user.Id);
                    var userDto = _mapper.Map<UserDto>(userWithRoles);
                    userDto.Roles = userWithRoles.UserRoles.Select(ur => ur.Role.Name).ToList();
                    userDtos.Add(userDto);
                }

                return userDtos;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener todos los usuarios: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<UserDto> UpdateUserAsync(int userId, UserUpdateRequest updateRequest)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new ApplicationException($"Usuario con ID {userId} no encontrado");

                // Verificar si el email ya está en uso por otro usuario
                if (updateRequest.Email != user.Email)
                {
                    var existingEmail = await _unitOfWork.Users.GetByEmailAsync(updateRequest.Email);
                    if (existingEmail != null)
                        throw new ApplicationException("El correo electrónico ya está registrado por otro usuario");
                }

                // Actualizar propiedades
                user.Name = updateRequest.Name;
                user.Email = updateRequest.Email;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.CompleteAsync();

                // Obtener usuario actualizado con roles
                var updatedUser = await _unitOfWork.Users.GetUserWithRolesAsync(userId);
                var userDto = _mapper.Map<UserDto>(updatedUser);
                userDto.Roles = updatedUser.UserRoles.Select(ur => ur.Role.Name).ToList();

                return userDto;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al actualizar usuario {userId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al actualizar usuario {userId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new ApplicationException($"Usuario con ID {userId} no encontrado");

                // Verificar si el usuario tiene órdenes
                var orders = await _unitOfWork.Orders.GetOrdersByUserAsync(userId);
                if (orders.Any())
                    throw new ApplicationException("No se puede eliminar un usuario con órdenes asociadas");

                // Eliminar roles de usuario
                var userWithRoles = await _unitOfWork.Users.GetUserWithRolesAsync(userId);
                foreach (var userRole in userWithRoles.UserRoles.ToList())
                {
                    await _unitOfWork.Users.DeleteAsync(userRole);
                }

                // Eliminar usuario
                await _unitOfWork.Users.DeleteAsync(user);
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al eliminar usuario {userId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al eliminar usuario {userId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<UserDto> AssignRoleToUserAsync(UserRoleRequest request)
        {
            try
            {
                var user = await _unitOfWork.Users.GetUserWithRolesAsync(request.UserId);
                if (user == null)
                    throw new ApplicationException($"Usuario con ID {request.UserId} no encontrado");

                var role = await _unitOfWork.Roles.GetByNameAsync(request.RoleName);
                if (role == null)
                    throw new ApplicationException($"Rol {request.RoleName} no encontrado");

                // Verificar si el usuario ya tiene el rol
                if (user.UserRoles.Any(ur => ur.Role.Name == request.RoleName))
                    throw new ApplicationException($"El usuario ya tiene el rol {request.RoleName}");

                // Asignar rol
                var userRole = new UserRole
                {
                    UserId = request.UserId,
                    RoleId = role.Id
                };

                await _unitOfWork.Users.AddAsync(userRole);
                await _unitOfWork.CompleteAsync();

                // Obtener usuario actualizado con roles
                var updatedUser = await _unitOfWork.Users.GetUserWithRolesAsync(request.UserId);
                var userDto = _mapper.Map<UserDto>(updatedUser);
                userDto.Roles = updatedUser.UserRoles.Select(ur => ur.Role.Name).ToList();

                return userDto;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al asignar rol a usuario {request.UserId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al asignar rol a usuario {request.UserId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<UserDto> RemoveRoleFromUserAsync(UserRoleRequest request)
        {
            try
            {
                var user = await _unitOfWork.Users.GetUserWithRolesAsync(request.UserId);
                if (user == null)
                    throw new ApplicationException($"Usuario con ID {request.UserId} no encontrado");

                var role = await _unitOfWork.Roles.GetByNameAsync(request.RoleName);
                if (role == null)
                    throw new ApplicationException($"Rol {request.RoleName} no encontrado");

                // Encontrar la relación usuario-rol
                var userRole = user.UserRoles.FirstOrDefault(ur => ur.Role.Name == request.RoleName);
                if (userRole == null)
                    throw new ApplicationException($"El usuario no tiene el rol {request.RoleName}");

                // Verificar que el usuario tenga al menos un rol después de eliminar este
                if (user.UserRoles.Count == 1)
                    throw new ApplicationException("El usuario debe tener al menos un rol");

                // Eliminar rol
                await _unitOfWork.Users.DeleteAsync(userRole);
                await _unitOfWork.CompleteAsync();

                // Obtener usuario actualizado con roles
                var updatedUser = await _unitOfWork.Users.GetUserWithRolesAsync(request.UserId);
                var userDto = _mapper.Map<UserDto>(updatedUser);
                userDto.Roles = updatedUser.UserRoles.Select(ur => ur.Role.Name).ToList();

                return userDto;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al eliminar rol de usuario {request.UserId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al eliminar rol de usuario {request.UserId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string roleName)
        {
            try
            {
                var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
                if (role == null)
                    throw new ApplicationException($"Rol {roleName} no encontrado");

                var users = await _unitOfWork.Users.GetUsersByRoleAsync(roleName);
                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var userWithRoles = await _unitOfWork.Users.GetUserWithRolesAsync(user.Id);
                    var userDto = _mapper.Map<UserDto>(userWithRoles);
                    userDto.Roles = userWithRoles.UserRoles.Select(ur => ur.Role.Name).ToList();
                    userDtos.Add(userDto);
                }

                return userDtos;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al obtener usuarios por rol {roleName}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener usuarios por rol {roleName}: {ex.Message}", ex);
                throw;
            }
        }
    }
}