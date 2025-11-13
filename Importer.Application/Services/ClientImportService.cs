using Importer.Application.Interfaces;
using Importer.Domain.Enums;
using System.Data;

namespace Importer.Application.Services
{
    public class ClientImportService : IClientImportService
    {
        private readonly IExcelBulkImporter _importer;

        public ClientImportService(IExcelBulkImporter importer)
        {
            _importer = importer;
        }

        public Task ImportClientsAsync(DataLoaderName dataLoaderName, CancellationToken ct = default)
        {
            return _importer.ImportAsync(
                dataLoaderName,
                buildTable: () =>
                {
                    var dt = new DataTable();
                    dt.Columns.Add("FirstName", typeof(string));
                    dt.Columns.Add("LastName", typeof(string));
                    return dt;
                },
                addRow: (cells, dt) =>
                {
                    var full = (cells.Count > 0 ? cells[0]?.ToString() : null) ?? string.Empty;
                    var (first, last) = SplitName(full);

                    if (string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(last))
                        return; 

                    var dr = dt.NewRow();
                    dr["FirstName"] = first;
                    dr["LastName"] = last;
                    dt.Rows.Add(dr);
                },
                ct: ct);
        }

        private static (string First, string Last) SplitName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return ("", "");

            var name = input.Trim();

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return (parts[0], "");

            return (parts[0], parts[1]);
        }
    }
}
    

       

