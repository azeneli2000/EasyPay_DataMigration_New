using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkOrderManagement.API.Domain;
using WorkOrderManagement.API.Infrastructure;

namespace WorkOrderManagement.API.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("WorkOrdersTestDb" );
                });

                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();

                db.Database.EnsureCreated();
                SeedTestData(db);
            });
        }

        private static void SeedTestData(AppDbContext db)
        {
            db.WorkOrders.RemoveRange(db.WorkOrders);
            db.Clients.RemoveRange(db.Clients);
            db.Technicians.RemoveRange(db.Technicians);
            db.SaveChanges();

            var client1 = new Client { ClientId = 1, FirstName = "Client A", LastName = "Client A1" };
            var client2 = new Client { ClientId = 2, FirstName = "Client B", LastName = "Client A2" };

            var tech1 = new Technician { TechnicianId = 1, FirstName = "John", LastName = "Doe" };
            var tech2 = new Technician { TechnicianId = 2, FirstName = "Jane", LastName = "Smith" };

            db.Clients.AddRange(client1, client2);
            db.Technicians.AddRange(tech1, tech2);

            var wo1 = new WorkOrder
            {
                WorkOrderId = 1,
                ClientId = client1.ClientId,
                TechnicianId = tech1.TechnicianId,
                Information = "WO 1",
                Date = new DateTime(2025, 1, 1),
                Total = 100m
            };
            var wo2 = new WorkOrder
            {
                WorkOrderId = 2,
                ClientId = client1.ClientId,
                TechnicianId = tech2.TechnicianId,
                Information = "WO 2",
                Date = new DateTime(2025, 1, 2),
                Total = 200m
            };
            var wo3 = new WorkOrder
            {
                WorkOrderId = 3,
                ClientId = client2.ClientId,
                TechnicianId = tech1.TechnicianId,
                Information = "WO 3",
                Date = new DateTime(2025, 1, 3),
                Total = 300m
            };

            db.WorkOrders.AddRange(wo1, wo2, wo3);
            db.SaveChanges();
        }     

    }
}
