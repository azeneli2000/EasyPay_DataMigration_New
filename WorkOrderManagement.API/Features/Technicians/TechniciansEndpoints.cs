using Microsoft.EntityFrameworkCore;
using WorkOrderManagement.API.Domain;
using WorkOrderManagement.API.Infrastructure;

namespace WorkOrderManagement.API.Features.Technicians
{
    public static class TechniciansEndpoints
    {
        public static IEndpointRouteBuilder MapTechniciansEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/technicians");

            group.MapGet("/", async (AppDbContext db) =>
                Results.Ok(await db.Technicians.ToListAsync()));

            group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
            {
                var tech = await db.Technicians.FindAsync(id);
                return tech is null ? Results.NotFound() : Results.Ok(tech);
            });

            group.MapPost("/", async (Technician tech, AppDbContext db) =>
            {
                db.Technicians.Add(tech);
                await db.SaveChangesAsync();
                return Results.Created($"/api/technicians/{tech.TechnicianId}", tech);
            });

            group.MapPut("/{id:int}", async (int id, Technician updated, AppDbContext db) =>
            {
                var tech = await db.Technicians.FindAsync(id);
                if (tech is null) return Results.NotFound();

                tech.FirstName = updated.FirstName;
                tech.LastName = updated.LastName;

                await db.SaveChangesAsync();
                return Results.Ok(tech);
            });

            group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
            {
                var tech = await db.Technicians.FindAsync(id);
                if (tech is null) return Results.NotFound();

                db.Technicians.Remove(tech);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            return routes;
        }
    }
}
