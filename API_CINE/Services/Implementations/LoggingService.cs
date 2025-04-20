using API_CINE.Models.Domain;
using API_CINE.Repositories.Interfaces;
using API_CINE.Services.Interfaces;

namespace API_CINE.Services.Implementations
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public LoggingService(
            ILogger<LoggingService> logger,
            IUnitOfWork unitOfWork = null) // Inyección opcional para evitar dependencia circular
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public void LogError(string message, Exception ex = null)
        {
            if (ex != null)
            {
                _logger.LogError(ex, message);
            }
            else
            {
                _logger.LogError(message);
            }
        }

        public void LogAudit(string action, string entityName, int? entityId, string userId, string details, string ipAddress)
        {
            try
            {
                // Registrar en el log
                _logger.LogInformation($"AUDIT: {action} {entityName} {entityId} by {userId} - {details} from {ipAddress}");

                // Si el UnitOfWork está disponible, registrar en la base de datos
                if (_unitOfWork != null)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            var auditLog = new AuditLog
                            {
                                Action = action,
                                EntityName = entityName,
                                EntityId = entityId,
                                UserId = userId,
                                Details = details,
                                IpAddress = ipAddress,
                                CreatedAt = DateTime.UtcNow
                            };

                            await _unitOfWork.AuditLogs.AddAsync(auditLog);
                            await _unitOfWork.CompleteAsync();
                        }
                        catch (Exception ex)
                        {
                            // No propagar errores - solo registrar el problema
                            _logger.LogError(ex, $"Error al guardar registro de auditoría: {ex.Message}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                // No propagar errores - solo registrar el problema
                _logger.LogError(ex, $"Error en LogAudit: {ex.Message}");
            }
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(string entityName, int? entityId)
        {
            try
            {
                if (_unitOfWork == null)
                    return new List<AuditLog>();

                return await _unitOfWork.AuditLogs.GetLogsByEntityAsync(entityName, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener registros de auditoría para {entityName} {entityId}: {ex.Message}");
                return new List<AuditLog>();
            }
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByUserAsync(string userId)
        {
            try
            {
                if (_unitOfWork == null)
                    return new List<AuditLog>();

                return await _unitOfWork.AuditLogs.GetLogsByUserAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener registros de auditoría para usuario {userId}: {ex.Message}");
                return new List<AuditLog>();
            }
        }
    }
}