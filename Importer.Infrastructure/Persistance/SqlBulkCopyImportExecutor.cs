using Importer.Application.Interfaces;
using Importer.Domain.Enums;
using Importer.Infrastructure.Configurations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Importer.Infrastructure.Persistance
{
    public class SqlBulkCopyImportExecutor : IDbImportExecutor
    {
        private readonly IConfiguration _config;

        public SqlBulkCopyImportExecutor(IConfiguration config)
        {
            _config = config;
        }

        public async Task ExecuteAsync(DataTable table, bool valid)
        {
            if (table == null || table.Rows.Count == 0)
                return;

            var connectionString =_config.GetConnectionString();

            var importConfig = _config.GetImportConfiguration(DataLoaderName.WorkOrdersFromExcel);

            var destTable = valid
                ? importConfig.Bulk.ValidTable
                : importConfig.Bulk.ErrorTable;

            if (string.IsNullOrWhiteSpace(destTable))            
                throw new InvalidOperationException($"Bulk copy not configured for import '{DataLoaderName.WorkOrdersFromExcel}'.");
            

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var bulk = new SqlBulkCopy(conn)
            {
                DestinationTableName = destTable,
                BatchSize = 5000,
                BulkCopyTimeout = 0
            };

            foreach (DataColumn col in table.Columns)
            {
                bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);
            }

            await bulk.WriteToServerAsync(table);
        }
    }
}
