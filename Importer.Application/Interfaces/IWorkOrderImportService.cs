using Importer.Application.DTO;
using Importer.Domain.Enums;

namespace Importer.Application.Interfaces
{
    public interface IWorkOrderImportService
    {
        Task<ImportResult> ImportAsync(DataLoaderName dataLoaderName,CancellationToken cancellationToken);
    }
}
