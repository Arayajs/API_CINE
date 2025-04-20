using API_CINE.Models.Domain;

namespace API_CINE.Services.Interfaces
{
    public interface ILoggingService
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
        void LogAudit(string action, string entityName, int? entityId, string userId, string details, string ipAddress);
        Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(string entityName, int? entityId);
        Task<IEnumerable<AuditLog>> GetAuditLogsByUserAsync(string userId);
    }
}
