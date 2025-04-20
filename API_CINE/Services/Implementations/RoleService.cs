using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using API_CINE.Repositories.Interfaces;
using API_CINE.Services.Interfaces;
using AutoMapper;

namespace API_CINE.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;

        public RoleService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggingService loggingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _loggingService = loggingService;
        }

        public async Task<RoleDto> GetRoleByIdAsync(int roleId)
        {
            try
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
                return _mapper.Map<RoleDto>(role);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener rol por ID {roleId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<RoleDto> GetRoleByNameAsync(string roleName)
        {
            try
            {
                var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
                return _mapper.Map<RoleDto>(role);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener rol por nombre {roleName}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _unitOfWork.Roles.GetAllAsync();
                return _mapper.Map<IEnumerable<RoleDto>>(roles);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al obtener todos los roles: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<RoleDto> CreateRoleAsync(RoleRequest request)
        {
            try
            {
                // Verificar si el rol ya existe
                var existingRole = await _unitOfWork.Roles.GetByNameAsync(request.Name);
                if (existingRole != null)
                    throw new ApplicationException($"El rol {request.Name} ya existe");

                var role = new Role
                {
                    Name = request.Name,
                    Description = request.Description
                };

                await _unitOfWork.Roles.AddAsync(role);
                await _unitOfWork.CompleteAsync();

                return _mapper.Map<RoleDto>(role);
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al crear rol: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al crear rol: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<RoleDto> UpdateRoleAsync(int roleId, RoleRequest request)
        {
            try
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
                if (role == null)
                    throw new ApplicationException($"Rol con ID {roleId} no encontrado");

                // No permitir cambiar el nombre si es uno de los roles predefinidos
                if ((role.Name == "Administrator" || role.Name == "Customer") &&
                    role.Name != request.Name)
                {
                    throw new ApplicationException("No se puede cambiar el nombre de los roles predefinidos");
                }

                // Verificar si el nombre nuevo ya existe en otro rol
                if (role.Name != request.Name)
                {
                    var existingRole = await _unitOfWork.Roles.GetByNameAsync(request.Name);
                    if (existingRole != null && existingRole.Id != roleId)
                        throw new ApplicationException($"El rol {request.Name} ya existe");
                }

                // Actualizar propiedades
                role.Name = request.Name;
                role.Description = request.Description;

                await _unitOfWork.Roles.UpdateAsync(role);
                await _unitOfWork.CompleteAsync();

                return _mapper.Map<RoleDto>(role);
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al actualizar rol {roleId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al actualizar rol {roleId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            try
            {
                var role = await _unitOfWork.Roles.GetRoleWithUsersAsync(roleId);
                if (role == null)
                    throw new ApplicationException($"Rol con ID {roleId} no encontrado");

                // No permitir eliminar roles predefinidos
                if (role.Name == "Administrator" || role.Name == "Customer")
                    throw new ApplicationException("No se pueden eliminar roles predefinidos del sistema");

                // Verificar si el rol está asociado a usuarios
                if (role.UserRoles != null && role.UserRoles.Count > 0)
                    throw new ApplicationException("No se puede eliminar un rol que tiene usuarios asociados");

                await _unitOfWork.Roles.DeleteAsync(role);
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error al eliminar rol {roleId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al eliminar rol {roleId}: {ex.Message}", ex);
                throw;
            }
        }
    }
}