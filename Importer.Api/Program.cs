using Importer.Application.Extensions;
using Importer.Infrastructure.Configurations;
using Importer.Infrastructure.Extensions;
using Importer.Infrastructure.Sql;
using Importer.Infrastructure.Sql.Scripts;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddInfrastructureServices();

builder.Services.AddApplicationServices();

var app = builder.Build();


// Note : This code exists only to avoid manual creation during testing. Is not a good practice creating SP,TVP this way
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var executor = scope.ServiceProvider.GetRequiredService<ISqlScriptExecutor>();

    var imports = config.GetImportConfigurations();
    var scripts = BootstrapScriptFactory.FromConfigs(imports);
    await executor.ExecuteManyAsync(scripts);
}

app.MapControllers();

app.Run();
