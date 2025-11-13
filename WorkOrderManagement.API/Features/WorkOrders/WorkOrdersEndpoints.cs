using Microsoft.EntityFrameworkCore;
using WorkOrderManagement.API.Domain;
using WorkOrderManagement.API.Infrastructure;

namespace WorkOrderManagement.API.Features.WorkOrders
{
    public static class WorkOrdersEndpoints
    {
        public static IEndpointRouteBuilder MapWorkOrdersEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/workorders");

            group.MapGet("/", async (AppDbContext db) =>
            {
                var items = await db.WorkOrders
                    .Include(w => w.Client)
                    .Include(w => w.Technician)
                    .OrderByDescending(w => w.Date)
                    .ToListAsync();

                return Results.Ok(items);
            });

            group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
            {
                var wo = await db.WorkOrders
                    .Include(w => w.Client)
                    .Include(w => w.Technician)
                    .FirstOrDefaultAsync(w => w.WorkOrderId == id);

                return wo is null ? Results.NotFound() : Results.Ok(wo);
            });

            group.MapPost("/", async (CreateWorkOrderRequest req, AppDbContext db) =>
            {
                var clientExists = await db.Clients.AnyAsync(c => c.ClientId == req.ClientId);
                var techExists = await db.Technicians.AnyAsync(t => t.TechnicianId == req.TechnicianId);
                if (!clientExists || !techExists)
                    return Results.BadRequest("Client or technician does not exist.");

                var wo = new WorkOrder
                {
                    TechnicianId = req.TechnicianId,
                    ClientId = req.ClientId,
                    Information = req.Information ?? "",
                    Date = req.Date,
                    Total = req.Total
                };

                db.WorkOrders.Add(wo);
                await db.SaveChangesAsync();
                return Results.Created($"/api/workorders/{wo.WorkOrderId}", wo);
            });

            group.MapPut("/{id:int}", async (int id, UpdateWorkOrderRequest req, AppDbContext db) =>
            {
                var wo = await db.WorkOrders.FindAsync(id);
                if (wo is null) return Results.NotFound();

                wo.TechnicianId = req.TechnicianId;
                wo.ClientId = req.ClientId;
                wo.Information = req.Information ?? "";
                wo.Date = req.Date;
                wo.Total = req.Total;

                await db.SaveChangesAsync();
                return Results.Ok(wo);
            });

            group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
            {
                var wo = await db.WorkOrders.FindAsync(id);
                if (wo is null) return Results.NotFound();

                db.WorkOrders.Remove(wo);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            group.MapGet("/search", async (
                int? technicianId,
                int? clientId,
                DateTime? from,
                DateTime? to,
                AppDbContext db) =>
            {
                var query = db.WorkOrders
                    .Include(w => w.Client)
                    .Include(w => w.Technician)
                    .AsQueryable();

                if (technicianId.HasValue)
                    query = query.Where(w => w.TechnicianId == technicianId.Value);

                if (clientId.HasValue)
                    query = query.Where(w => w.ClientId == clientId.Value);

                if (from.HasValue)
                    query = query.Where(w => w.Date >= from.Value);

                if (to.HasValue)
                    query = query.Where(w => w.Date <= to.Value);

                var items = await query
                    .OrderByDescending(w => w.Date)
                    .ToListAsync();

                return Results.Ok(items);
            });

            return routes;
        }


    }

    public record CreateWorkOrderRequest(int TechnicianId, int ClientId, string? Information, DateTime Date, decimal Total);

    public record UpdateWorkOrderRequest(int TechnicianId, int ClientId, string? Information, DateTime Date, decimal Total);
}
