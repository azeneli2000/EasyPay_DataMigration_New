using Microsoft.EntityFrameworkCore;
using WorkOrderManagement.API.Infrastructure;

namespace WorkOrderManagement.API.Features.Clients
{
    public static  class ClientsEndpoints
    {
        public static IEndpointRouteBuilder MapClientsEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/clients");

            group.MapGet("/", async (AppDbContext db) =>
            {
                var items = await db.Clients
                    .OrderBy(c => c.ClientId)
                    .ToListAsync();
                return Results.Ok(items);
            });

            //




            return routes;
        }
    }
}
