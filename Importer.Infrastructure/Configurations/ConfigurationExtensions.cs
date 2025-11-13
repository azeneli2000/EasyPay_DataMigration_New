using Importer.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace Importer.Infrastructure.Configurations
{
    public static class ConfigurationExtensions
    {
        public static Dictionary<string, ImportConfig> GetImportConfigurations(this IConfiguration configuration)
        {
            var result = new Dictionary<string, ImportConfig>();
            var section = configuration.GetSection("ImportConfigurations");

            foreach (var child in section.GetChildren())
            {
                var importConfig = new ImportConfig();
                child.Bind(importConfig);
                result[child.Key] = importConfig;
            }

            return result;
        }

        public static ImportConfig? GetImportConfiguration(
         this IConfiguration configuration,
         DataLoaderName dataLoaderName)
        {
            var key = dataLoaderName.ToString();
            var sectionPath = $"ImportConfigurations:{key}";
            var section = configuration.GetSection(sectionPath);
            if (!section.Exists())
                throw new KeyNotFoundException($"Configuration for data loader '{dataLoaderName}' not found under '{sectionPath}'.");

            var config = new ImportConfig();
            section.Bind(config);
            return config;
        }

        public static string GetConnectionString(this IConfiguration configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            var connectionString = configuration.GetConnectionString("db");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string 'db' not found or is empty. ");

            return connectionString;
        }
        public static string GetReportPath(this IConfiguration configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));
            var sectionPath = "ReportPath";

            var section = configuration.GetSection(sectionPath).Value ?? throw new Exception("Report path not found");

            return section;
        }
    }
}
