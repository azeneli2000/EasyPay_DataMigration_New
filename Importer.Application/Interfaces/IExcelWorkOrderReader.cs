using Importer.Domain.Enums;

namespace Importer.Application.Interfaces
{
    public interface IExcelStreamReader
    {
        IAsyncEnumerable<T> ReadAsync<T>(DataLoaderName dataLoaderName,
            Func<IReadOnlyList<object?>, T> mapRow,
            int headerRows = 1,
            CancellationToken ct = default);
    }
}
