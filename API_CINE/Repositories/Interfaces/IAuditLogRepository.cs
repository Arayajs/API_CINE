using API_CINE.Models.Domain;

namespace API_CINE.Repositories.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetLogsByEntityAsync(string entityName, int? entityId);
        Task<IEnumerable<AuditLog>> GetLogsByUserAsync(string userId);
        Task<IEnumerable<AuditLog>> GetLogsByActionAsync(string action);
        Task<IEnumerable<AuditLog>> GetLogsByDateRangeAsync(DateTime start, DateTime end);
    }
}
