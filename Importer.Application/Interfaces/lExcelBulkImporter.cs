using Importer.Domain.Enums;
using System.Data;

namespace Importer.Application.Interfaces
{
    public interface IExcelBulkImporter
    {
        Task ImportAsync(DataLoaderName dataLoaderName, Func<DataTable> buildTable, Action<IReadOnlyList<object?>, DataTable> addRow, CancellationToken ct = default);
    }
}
