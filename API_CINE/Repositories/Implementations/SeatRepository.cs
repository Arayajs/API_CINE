using API_CINE.Data;
using API_CINE.Models.Domain;
using API_CINE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_CINE.Repositories.Implementations
{
    public class SeatRepository : Repository<Seat>, ISeatRepository
    {
        public SeatRepository(CinemaDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Seat>> GetSeatsByCinemaHallAsync(int hallId)
        {
            return await _dbSet
                .Where(s => s.CinemaHallId == hallId && s.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Seat>> GetAvailableSeatsForScreeningAsync(int screeningId)
        {
            // Obtener todos los asientos de la sala
            var screening = await _context.MovieScreenings
                .Include(s => s.CinemaHall)
                .FirstOrDefaultAsync(s => s.Id == screeningId);

            if (screening == null)
                return new List<Seat>();

            var hallId = screening.CinemaHallId;

            // Obtener los asientos ya reservados para esta proyección
            var reservedSeatIds = await _context.Tickets
                .Where(t => t.MovieScreeningId == screeningId)
                .Select(t => t.SeatId)
                .ToListAsync();

            // Obtener los asientos disponibles (no reservados)
            return await _dbSet
                .Where(s => s.CinemaHallId == hallId && s.IsActive && !reservedSeatIds.Contains(s.Id))
                .ToListAsync();
        }

        public async Task<bool> IsSeatAvailableForScreeningAsync(int seatId, int screeningId)
        {
            // Verificar si el asiento existe y está activo
            var seat = await _dbSet.FindAsync(seatId);
            if (seat == null || !seat.IsActive)
                return false;

            // Verificar si la proyección existe
            var screening = await _context.MovieScreenings.FindAsync(screeningId);
            if (screening == null || !screening.IsActive || screening.CinemaHallId != seat.CinemaHallId)
                return false;

            // Verificar si el asiento ya está reservado para esta proyección
            var isReserved = await _context.Tickets
                .AnyAsync(t => t.MovieScreeningId == screeningId && t.SeatId == seatId);

            return !isReserved;
        }
    }

}
