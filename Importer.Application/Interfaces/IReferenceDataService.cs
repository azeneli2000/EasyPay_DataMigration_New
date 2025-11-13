using Importer.Domain.Entities;

namespace Importer.Application.Interfaces
{
    public interface IReferenceDataService
    {
        Task<List<Client>> LoadClientsAsync();
        Task<Dictionary<string, int>> LoadTechniciansAsync();
    }
}
