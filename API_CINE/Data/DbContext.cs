using API_CINE.Models.Domain;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace API_CINE.Data
{
    public class CinemaDbContext : DbContext
    {
        public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<CinemaHall> CinemaHalls { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieScreening> MovieScreenings { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuraciones de relaciones y restricciones

            // User - Role (muchos a muchos)
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cinema - CinemaHall (uno a muchos)
            modelBuilder.Entity<CinemaHall>()
                .HasOne(h => h.Cinema)
                .WithMany(c => c.CinemaHalls)
                .HasForeignKey(h => h.CinemaId)
                .OnDelete(DeleteBehavior.Cascade);

            // CinemaHall - MovieScreening (uno a muchos)
            modelBuilder.Entity<MovieScreening>()
                .HasOne(s => s.CinemaHall)
                .WithMany(h => h.MovieScreenings)
                .HasForeignKey(s => s.CinemaHallId)
                .OnDelete(DeleteBehavior.Restrict);

            // Movie - MovieScreening (uno a muchos)
            modelBuilder.Entity<MovieScreening>()
                .HasOne(s => s.Movie)
                .WithMany(m => m.MovieScreenings)
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            // Especificación del tipo de dato para TicketPrice
            modelBuilder.Entity<MovieScreening>()
                .Property(ms => ms.TicketPrice)
                .HasColumnType("decimal(10, 2)");

            // Seat - CinemaHall (uno a muchos)
            modelBuilder.Entity<Seat>()
                .HasOne(s => s.CinemaHall)
                .WithMany(h => h.Seats)
                .HasForeignKey(s => s.CinemaHallId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Order (uno a muchos)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Especificación del tipo de dato para TotalAmount
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(10, 2)");

            // Order - Ticket (uno a muchos)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Order)
                .WithMany(o => o.Tickets)
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // MovieScreening - Ticket (uno a muchos)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.MovieScreening)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.MovieScreeningId)
                .OnDelete(DeleteBehavior.Restrict);

            // Especificación del tipo de dato para Price
            modelBuilder.Entity<Ticket>()
                .Property(t => t.Price)
                .HasColumnType("decimal(10, 2)");

            // Seat - Ticket (uno a muchos)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            // Semilla de datos para roles (usando valores estáticos)
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Administrator", Description = "Administrador del sistema", CreatedAt = new DateTime(2025, 04, 05, 21, 45, 00, DateTimeKind.Utc) },
                new Role { Id = 2, Name = "Customer", Description = "Cliente del sistema", CreatedAt = new DateTime(2025, 04, 05, 21, 45, 00, DateTimeKind.Utc) }
            );

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => x.Entity is Entity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((Entity)entity.Entity).CreatedAt = DateTime.UtcNow;
                }

                if (entity.State == EntityState.Modified)
                {
                    ((Entity)entity.Entity).UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
