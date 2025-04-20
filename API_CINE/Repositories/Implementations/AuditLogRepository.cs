using API_CINE.Data;
using API_CINE.Models.Domain;
using API_CINE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_CINE.Repositories.Implementations
{
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(CinemaDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AuditLog>> GetLogsByEntityAsync(string entityName, int? entityId)
        {
            return await _dbSet
                .Where(l => l.EntityName == entityName && (entityId == null || l.EntityId == entityId))
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetLogsByUserAsync(string userId)
        {
            return await _dbSet
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetLogsByActionAsync(string action)
        {
            return await _dbSet
                .Where(l => l.Action == action)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetLogsByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _dbSet
                .Where(l => l.CreatedAt >= start && l.CreatedAt <= end)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }
    }
}
