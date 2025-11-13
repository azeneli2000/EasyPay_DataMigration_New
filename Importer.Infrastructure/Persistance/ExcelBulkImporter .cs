using Importer.Application.Interfaces;
using Importer.Domain.Enums;
using Importer.Infrastructure.Configurations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Importer.Infrastructure.Persistance
{
    
    public class ExcelBulkImporter : IExcelBulkImporter
    {
        private readonly IExcelStreamReader _excelReader;
        private readonly IConfiguration _configuration;

        public ExcelBulkImporter(IExcelStreamReader excelReader, IConfiguration configuration)
        {
            _configuration = configuration;
            _excelReader = excelReader ?? throw new ArgumentNullException(nameof(excelReader));
        }

        public async Task ImportAsync(  
            DataLoaderName dataLoaderName,
            Func<DataTable> buildTable,
            Action<IReadOnlyList<object?>, DataTable> addRow,
            CancellationToken ct = default)
        {
            var connectionString = _configuration.GetConnectionString();

            var importConfig = _configuration.GetImportConfiguration(dataLoaderName);

            var table = buildTable();
            if (table.Columns.Count == 0)
                throw new InvalidOperationException("The provided DataTable has no columns. Define columns in buildTable().");

            await foreach (var cells in _excelReader.ReadAsync(dataLoaderName,
                mapRow: c => c,         
                headerRows: 1,
                ct: ct))
            {
                ct.ThrowIfCancellationRequested();
                addRow(cells, table);
            }

            if (table.Rows.Count == 0)
                return;

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);

            using var tx = conn.BeginTransaction();

            var options = SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.FireTriggers;

            using var bulk = new SqlBulkCopy(conn, options, tx)
            {
                DestinationTableName = importConfig.Bulk.ValidTable,
                BatchSize = table.Rows.Count,
                BulkCopyTimeout = 0
            };

            foreach (DataColumn col in table.Columns)
            {
                bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);
            }

            await bulk.WriteToServerAsync(table, ct);

            tx.Commit();
        }
    }
}

