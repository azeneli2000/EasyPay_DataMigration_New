using Microsoft.EntityFrameworkCore;
using WorkOrderManagement.API.Features.Clients;
using WorkOrderManagement.API.Features.Technicians;
using WorkOrderManagement.API.Features.WorkOrders;
using WorkOrderManagement.API.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("db"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    using (var sc = app.Services.CreateScope())
    {
        var db1 = sc.ServiceProvider.GetRequiredService<AppDbContext>();

        if (db1.Database.IsRelational())
        {
            db.Database.Migrate();
        }   
    }
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app. MapClientsEndpoints();
app.MapTechniciansEndpoints();
app.MapWorkOrdersEndpoints();
app.Run();
public partial class Program { }

