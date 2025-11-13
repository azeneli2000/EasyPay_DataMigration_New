using System.Data;

namespace Importer.Application.Interfaces
{
    public interface IDbImportExecutor
    {
        Task ExecuteAsync(DataTable table, bool valid);
    }
}
