using API_CINE.Data;
using API_CINE.Repositories.Interfaces;

namespace API_CINE.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CinemaDbContext _context;
        private bool _disposed = false;

        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;
        private ICinemaRepository _cinemaRepository;
        private ICinemaHallRepository _cinemaHallRepository;
        private IMovieRepository _movieRepository;
        private IMovieScreeningRepository _movieScreeningRepository;
        private ISeatRepository _seatRepository;
        private IOrderRepository _orderRepository;
        private ITicketRepository _ticketRepository;
        private IAuditLogRepository _auditLogRepository;

        public UnitOfWork(CinemaDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _userRepository ??= new UserRepository(_context);
        public IRoleRepository Roles => _roleRepository ??= new RoleRepository(_context);
        public ICinemaRepository Cinemas => _cinemaRepository ??= new CinemaRepository(_context);
        public ICinemaHallRepository CinemaHalls => _cinemaHallRepository ??= new CinemaHallRepository(_context);
        public IMovieRepository Movies => _movieRepository ??= new MovieRepository(_context);
        public IMovieScreeningRepository MovieScreenings => _movieScreeningRepository ??= new MovieScreeningRepository(_context);
        public ISeatRepository Seats => _seatRepository ??= new SeatRepository(_context);
        public IOrderRepository Orders => _orderRepository ??= new OrderRepository(_context);
        public ITicketRepository Tickets => _ticketRepository ??= new TicketRepository(_context);
        public IAuditLogRepository AuditLogs => _auditLogRepository ??= new AuditLogRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

