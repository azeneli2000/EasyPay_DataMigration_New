using Importer.Application.Interfaces;
using Importer.Domain.Interfaces;
using Importer.Infrastructure.Excel;
using Importer.Infrastructure.Matching;
using Importer.Infrastructure.Persistance;
using Importer.Infrastructure.Reporting;
using Importer.Infrastructure.Sql;
using Microsoft.Extensions.DependencyInjection;

namespace Importer.Infrastructure.Extensions
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddKeyedScoped<IDbImportExecutor, SpTvpImportExecutor>("Tvp");
            services.AddKeyedScoped<IDbImportExecutor, SqlBulkCopyImportExecutor>("Bulk");
            services.AddKeyedScoped<IDbImportExecutor, AdfIImportExecutor>("adf");

            services.AddScoped<IExcelStreamReader, ExcelStreamReader>();
            services.AddScoped<IExcelBulkImporter, ExcelBulkImporter>();
            services.AddScoped<IFuzzyMatcher, LevenshteinFuzzyMatcher>();
            services.AddScoped<ISqlScriptExecutor, SqlScriptExecutor>();
            services.AddScoped<IReportWriter, CsvBatchReportWriter>();
            services.AddScoped<IReferenceDataService, ReferenceDataService>();

            return services;
        }
    }
}
