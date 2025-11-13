namespace Importer.Infrastructure.Sql
{

        public interface ISqlScriptExecutor
        {
            Task ExecuteAsync(string sql, CancellationToken ct = default);
            Task ExecuteManyAsync(IEnumerable<string> scripts, CancellationToken ct = default);
        }
    }

