using Importer.Application.DTO;
using Importer.Application.Interfaces;
using Importer.Domain.Entities;
using Importer.Domain.Enums;
using Importer.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace Importer.Application.Services
{
    public class WorkOrderImportService : IWorkOrderImportService
    {
        private const int DefaultBatchSize = 5;//5000;

        private readonly IConfiguration _config;
        private readonly IReferenceDataService _refService;
        private readonly IExcelStreamReader _excelReader;
        private readonly IFuzzyMatcher _fuzzy;
        private readonly IDbImportExecutor _db;
        private readonly IReportWriter _report;

        public WorkOrderImportService(
            IConfiguration config,
            IReferenceDataService refService,
            IExcelStreamReader excelReader,
            IFuzzyMatcher fuzzy,
          IServiceProvider services,
          IReportWriter report)
        {
            _config = config;
            _refService = refService;
            _excelReader = excelReader;
            _fuzzy = fuzzy;
            var loaderKey = _config["ImportConfigurations:WorkOrdersFromExcel:Mode"] ?? "Bulk";
            _db = services.GetRequiredKeyedService<IDbImportExecutor>(loaderKey);
            _report = report;
        }





        public async Task<ImportResult> ImportAsync(DataLoaderName dataLoaderName, CancellationToken ct)
        {
            List<Client> clients = await _refService.LoadClientsAsync();
            Dictionary<string, int> technicians = await _refService.LoadTechniciansAsync();

            var successLines = new List<ImportSuccessLine>();
            var failLines = new List<ImportFailLine>();

            int total = 0;

            var valid = BuildValidTable();
            var invalid = BuildInvalidTable();

            var channel = Channel.CreateBounded<ImportBatch>(new BoundedChannelOptions(4)
            {
                SingleWriter = true,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait
            });

            var consumerTask = Task.Run(async () =>
            {
                await foreach (var batch in channel.Reader.ReadAllAsync(ct))
                {
                    await _db.ExecuteAsync(batch.Table, batch.IsValid);
                    await _report.WriteAsync(batch.Table, batch.IsValid);
                }
            }, ct);

            await foreach (var row in _excelReader.ReadAsync(
                  dataLoaderName: dataLoaderName,
                   mapRow: cells =>
                   {
                       var tech = cells.Count > 0 ? cells[0]?.ToString()?.Trim() ?? "" : "";
                       var notes = cells.Count > 1 ? cells[1]?.ToString()?.Trim() ?? "" : "";
                       var total = 0m;
                       if (cells.Count > 2)
                           decimal.TryParse(cells[2]?.ToString(), out total);

                       return new WorkOrderRow
                       {
                           TechnicianName = tech,
                           Notes = notes,
                           Total = total
                       };
                   },
                   headerRows: 1,
                   ct: ct
                   ))
            {
                ct.ThrowIfCancellationRequested();
                total++;
                var extractedDate = ExtractDate(row.Notes);
                var extractedClientName = ExtractClientName(row.Notes);

                var matchedClient = _fuzzy.Match(extractedClientName, clients);

                if (string.IsNullOrWhiteSpace(row.TechnicianName) ||
                !technicians.TryGetValue(row.TechnicianName, out var techId))
                {
                    AddInvalid(invalid,null, extractedClientName, extractedDate, row.Total, row.Notes, "Technician not found", failLines);
                }
                else
                {
                    //var extractedDate = ExtractDate(row.Notes);

                    if (matchedClient == null)
                    {
                        AddInvalid(invalid, row.TechnicianName,  extractedClientName, extractedDate, row.Total,  row.Notes,  "Client not matched",  failLines);
                    }
                    else
                    {
                        var info = CleanInformation(row.Notes, extractedClientName, extractedDate);

                        var validRow = valid.NewRow();
                        validRow["TechnicianId"] = techId;
                        validRow["ClientId"] = matchedClient.Id;
                        validRow["Information"] = info;
                        validRow["Date"] = extractedDate ?? DateTime.Today;
                        validRow["Total"] = row.Total;
                        valid.Rows.Add(validRow);

                        successLines.Add(new ImportSuccessLine
                        {
                            Technician = row.TechnicianName,
                            Client = matchedClient.FullName,
                            Date = extractedDate ?? DateTime.Today,
                            Total = row.Total
                        });
                    }
                }

                if (valid.Rows.Count >= DefaultBatchSize)
                {
                    var batchTable = CloneTableWithData(valid);
                    valid.Clear();
                    await channel.Writer.WriteAsync(new ImportBatch(batchTable, IsValid: true), ct);
                }

                if (invalid.Rows.Count >= DefaultBatchSize)
                {
                    var batchTable = CloneTableWithData(invalid);
                    invalid.Clear();
                    await channel.Writer.WriteAsync(new ImportBatch(batchTable, IsValid: false), ct);
                }
            }

            if (valid.Rows.Count > 0)
            {
                var batchTable = CloneTableWithData(valid);
                await channel.Writer.WriteAsync(new ImportBatch(batchTable, IsValid: true), ct);
            }

            if (invalid.Rows.Count > 0)
            {
                var batchTable = CloneTableWithData(invalid);
                await channel.Writer.WriteAsync(new ImportBatch(batchTable, IsValid: false), ct);
            }

            channel.Writer.Complete();
            await consumerTask;

            //await _report.WriteAsync(reportPath, successLines, failLines);

            return new ImportResult
            {
                TotalRows = total,
                SuccessCount = successLines.Count,
                FailedCount = failLines.Count,
            };
        }

        private readonly record struct ImportBatch(DataTable Table, bool IsValid);


        private static DataTable CloneTableWithData(DataTable source)
        {
            var clone = source.Clone();
            foreach (DataRow row in source.Rows)
            {
                clone.ImportRow(row);
            }
            return clone;
        }




        private static DataTable BuildValidTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("TechnicianId", typeof(int));
            dt.Columns.Add("ClientId", typeof(int));
            dt.Columns.Add("Information", typeof(string));
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("Total", typeof(decimal));
            return dt;
        }

        private static DataTable BuildInvalidTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("TechnicianName", typeof(string));
            dt.Columns.Add("ClientName", typeof(string));
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("Total", typeof(decimal));
            dt.Columns.Add("Reason", typeof(string));
            dt.Columns.Add("OriginalNotes", typeof(string));
            return dt;
        }

        private static void AddInvalid(DataTable table,
        string? technicianName,
        string? clientName,
        DateTime? date,
        decimal total,
        string notes,
        string reason,
        List<ImportFailLine> report)
        {
            var row = table.NewRow();

            row["TechnicianName"] = technicianName ?? "";
            row["ClientName"] = clientName ?? "";
            row["OriginalNotes"] = notes ?? "";
            row["Reason"] = reason;

            if (date.HasValue)
                row["Date"] = date.Value;
            else
                row["Date"] = DBNull.Value;

            row["Total"] = total;

            table.Rows.Add(row);

            report.Add(new ImportFailLine
            {
                Technician = technicianName ?? "",
                Client = clientName ?? "",
                Date = date,
                Total = total,
                Notes = notes ?? "",
                Reason = reason
            });
        }

        private static string? ExtractClientName(string notes)
        {
            if (string.IsNullOrWhiteSpace(notes)) return null;
            var idx = notes.IndexOf("klienti", StringComparison.OrdinalIgnoreCase);
            if (idx == -1) return null;
            var start = idx + "klienti".Length;
            var rest = notes.Substring(start).Trim();
            var dateIdx = rest.IndexOf("me daten", StringComparison.OrdinalIgnoreCase);
            if (dateIdx != -1)
                rest = rest.Substring(0, dateIdx).Trim();
            return rest.Trim(' ', '.', ',');
        }

        private static DateTime? ExtractDate(string notes)
        {
            if (string.IsNullOrWhiteSpace(notes)) return null;
            var m = Regex.Match(notes, @"\b(\d{1,2})/(\d{1,2})/(\d{4})\b");
            if (m.Success && DateTime.TryParse(m.Value, out var dt))
                return dt;
            return null;
        }

        private static string CleanInformation(string notes, string? clientName, DateTime? date)
        {
            var info = notes;
            if (!string.IsNullOrEmpty(clientName))
                info = info.Replace(clientName, "", StringComparison.OrdinalIgnoreCase);
            if (date.HasValue)
            {
                var ds = date.Value.ToString("dd/MM/yyyy");
                info = info.Replace(ds, "", StringComparison.OrdinalIgnoreCase);
                info = info.Replace("me daten", "", StringComparison.OrdinalIgnoreCase);
            }
            return info.Trim();
        }

    }
}
