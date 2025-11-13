using Microsoft.EntityFrameworkCore;
using WorkOrderManagement.API.Domain;

namespace WorkOrderManagement.API.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Technician> Technicians => Set<Technician>();
        public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>()
                .ToTable("Clients")
                .HasKey(x => x.ClientId);

            modelBuilder.Entity<Technician>()
                .ToTable("Technicians")
                .HasKey(x => x.TechnicianId);

            modelBuilder.Entity<WorkOrder>()
                .ToTable("WorkOrders")
                .HasKey(x => x.WorkOrderId);

            modelBuilder.Entity<WorkOrder>()
                .HasOne(x => x.Client)
                .WithMany()
                .HasForeignKey(x => x.ClientId);

            modelBuilder.Entity<WorkOrder>()
                .HasOne(x => x.Technician)
                .WithMany()
                .HasForeignKey(x => x.TechnicianId);
        }
    }
}
