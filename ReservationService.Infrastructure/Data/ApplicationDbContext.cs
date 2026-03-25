using Microsoft.EntityFrameworkCore;
using ReservationService.Domain.Common;
using ReservationService.Domain.Reservations;
using ReservationService.Domain.Tables;
using ReservationService.Domain.Users;
using ReservationService.Infrastructure.Data.Configurations;

namespace ReservationService.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Table> Tables { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<OutboxMessage> outboxMessages { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base (options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new TableConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new ReservationConfiguration());
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        }
    }
}
