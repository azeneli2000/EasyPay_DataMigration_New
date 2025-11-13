using Importer.Application.Interfaces;
using System.Data;

namespace Importer.Infrastructure.Persistance
{
    public class AdfIImportExecutor : IDbImportExecutor
    {
        public Task ExecuteAsync(DataTable table, bool valid)
        {
            //Eg send message to event grid to trigger ADF pipeline 
            throw new NotImplementedException();
        }
    }
}
