using Importer.Application.Interfaces;
using Importer.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Importer.Application.Extensions
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ITechnicianImportService, TechnicianImportService>();
            services.AddScoped<IClientImportService, ClientImportService>();
            services.AddScoped<IWorkOrderImportService, WorkOrderImportService>();

            return services;
        }
    }
}
