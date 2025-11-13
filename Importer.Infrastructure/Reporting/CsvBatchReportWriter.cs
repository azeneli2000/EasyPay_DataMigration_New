
using Importer.Application.Interfaces;
using Importer.Infrastructure.Configurations;
using Importer.Infrastructure.Persistance;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text;

namespace Importer.Infrastructure.Reporting
{

    public class CsvBatchReportWriter : IReportWriter
    {
        private static readonly SemaphoreSlim _lock = new(1, 1);
        private readonly string _path;
        private readonly Dictionary<int, string> _technicianNames;
        private readonly Dictionary<int, string> _clientNames;

        public CsvBatchReportWriter(IConfiguration configuration, IReferenceDataService referenceDataService)
        {
            string basePath = configuration.GetReportPath();

            var dir = Path.GetDirectoryName(basePath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"report_{timestamp}.csv";

            _path = Path.Combine(dir!, filename);
            var nameToId = referenceDataService
           .LoadTechniciansAsync()
           .GetAwaiter()
           .GetResult();

            _technicianNames = nameToId
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            var clients = referenceDataService
                .LoadClientsAsync()
                .GetAwaiter()
                .GetResult();

            _clientNames = clients
                .ToDictionary(c => c.Id, c => c.FullName);
        }

        public async Task WriteAsync(DataTable table, bool valid)
        {
            if (table.Rows.Count == 0)
                return;

            var sb = new StringBuilder();
            var status = valid ? "SUCCESS" : "FAIL";

            foreach (DataRow row in table.Rows)
            {
                var line = valid
                    ? BuildValidLine(status, row)
                    : BuildInvalidLine(status, row);

                sb.AppendLine(line);
            }

            await _lock.WaitAsync();
            try
            {
                var fileExists = File.Exists(_path);

                using var stream = new FileStream(
                    _path,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.Read); 

                using var writer = new StreamWriter(stream, Encoding.UTF8);

                if (!fileExists)
                {
                    await writer.WriteLineAsync("Status,Technician,Client,Date,Total,Reason,Notes");
                }

                await writer.WriteAsync(sb.ToString());
                await writer.FlushAsync();
            }
            finally
            {
                _lock.Release();
            }
        }
        private static string Escape(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        private  string BuildValidLine(string status, DataRow row)
        {    
            var techIdStr = GetValue(row, "TechnicianId");
            var clientIdStr = GetValue(row, "ClientId");

            string technicianDisplay = techIdStr;
            if (int.TryParse(techIdStr, out var techId) &&
                _technicianNames.TryGetValue(techId, out var techName))
            {
                technicianDisplay = techName;
            }

            string clientDisplay = clientIdStr;
            if (int.TryParse(clientIdStr, out var clientId) &&
                _clientNames.TryGetValue(clientId, out var clientName))
            {
                clientDisplay = clientName;
            }

            var date = Escape(GetDateValue(row, "Date"));
            var total = Escape(GetValue(row, "Total"));

            return $"{status},{Escape(technicianDisplay)},{Escape(clientDisplay)},{date},{total},,";
        }

        private static string BuildInvalidLine(string status, DataRow row)
        {
            var tech = Escape(GetValue(row, "TechnicianName"));
            var client = Escape(GetValue(row, "ClientName"));
            var date = Escape(GetDateValue(row, "Date"));
            var total = Escape(GetValue(row, "Total"));
            var reason = Escape(GetValue(row, "Reason"));
            var notes = Escape(GetValue(row, "OriginalNotes"));

            return $"{status},{tech},{client},{date},{total},{reason},{notes}";

        }

        private static string GetValue(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName)
                ? row[columnName]?.ToString() ?? string.Empty
                : string.Empty;
        }

        private static string GetDateValue(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return string.Empty;

            if (row[columnName] == null || row[columnName] == DBNull.Value)
                return string.Empty;

            if (DateTime.TryParse(row[columnName].ToString(), out var dt))
                return dt.ToString("yyyy-MM-dd");

            return string.Empty;
        }
       

    }
}
