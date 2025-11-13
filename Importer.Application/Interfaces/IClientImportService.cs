using Importer.Domain.Enums;

namespace Importer.Application.Interfaces
{
    public interface IClientImportService
    {
        Task ImportClientsAsync(DataLoaderName dataLoaderName, CancellationToken ct);
    }
}
