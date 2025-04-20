namespace API_CINE.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }

    
        ICinemaRepository Cinemas { get; }
        ICinemaHallRepository CinemaHalls { get; }
        IMovieRepository Movies { get; }
        IMovieScreeningRepository MovieScreenings { get; }
        ISeatRepository Seats { get; }
        IOrderRepository Orders { get; }
        ITicketRepository Tickets { get; }
        IAuditLogRepository AuditLogs { get; }
       

        Task<int> CompleteAsync();
    }
}
