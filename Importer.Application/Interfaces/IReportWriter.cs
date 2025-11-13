using System.Data;

namespace Importer.Application.Interfaces
{
    public interface IReportWriter
    {
        Task WriteAsync(DataTable table, bool valid);
    }
}
