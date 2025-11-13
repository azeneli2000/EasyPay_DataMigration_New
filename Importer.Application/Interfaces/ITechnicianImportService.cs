using Importer.Domain.Enums;

namespace Importer.Application.Interfaces
{
    public interface ITechnicianImportService
    {
        Task ImportTechniciansAsync(DataLoaderName dataLoaderName, CancellationToken ct = default);
    }
}
