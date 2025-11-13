using Importer.Application.Interfaces;
using Importer.Domain.Enums;
using System.Data;

namespace Importer.Application.Services
{
    public class TechnicianImportService : ITechnicianImportService
    {
        private readonly IExcelBulkImporter _importer;

        public TechnicianImportService(IExcelBulkImporter importer)
        {
            _importer = importer;
        }

        public Task ImportTechniciansAsync(DataLoaderName dataLoaderName, CancellationToken ct = default)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                    if (cells.Count == 0)
                        return;

                    var full = cells[0]?.ToString()?.Trim();

                    if (string.IsNullOrWhiteSpace(full) ||
                        full.Equals("Technician", StringComparison.OrdinalIgnoreCase))
                        return;

                    if (!seen.Add(full))
                        return;

                    var (first, last) = SplitName(full);

                    var dr = dt.NewRow();
                    dr["FirstName"] = first;
                    dr["LastName"] = last;
                    dt.Rows.Add(dr);
                },
                ct: ct);
        }
        private static (string First, string Last) SplitName(string fullName)
        {
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return (parts[0], "");

            return (parts[0], parts[1]);
        }
    }
}
