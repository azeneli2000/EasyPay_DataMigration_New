
using Importer.Domain.Entities;

namespace Importer.Domain.Interfaces
{
    public interface IFuzzyMatcher
    {
        Client? Match(string? extractedName, List<Client> officialClients);
    }
}
