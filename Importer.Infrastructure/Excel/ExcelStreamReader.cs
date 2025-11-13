using ExcelDataReader;
using Importer.Application.Interfaces;
using Importer.Domain.Enums;
using Importer.Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;

namespace Importer.Infrastructure.Excel
{
    public class ExcelStreamReader : IExcelStreamReader
    {
        private readonly IConfiguration _configuration;
        public ExcelStreamReader(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async IAsyncEnumerable<T> ReadAsync<T>(DataLoaderName dataLoaderName,
            Func<IReadOnlyList<object?>, T> mapRow,
            int headerRows = 1,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            var importConfig = _configuration.GetImportConfiguration(dataLoaderName);

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var stream = File.Open(importConfig.ExcelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            int rowIndex = 0;

            do
            {
                while (reader.Read())
                {
                    if (ct.IsCancellationRequested)
                        yield break;

                    if (rowIndex++ < headerRows)
                        continue;

                    var cells = new List<object?>(reader.FieldCount);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        cells.Add(reader.GetValue(i));
                    }

                    var mapped = mapRow(cells);
                    if (mapped is not null)
                        yield return mapped;

                    await Task.Yield();
                }
                rowIndex = 0;

            } while (reader.NextResult());
        }
    }
}
