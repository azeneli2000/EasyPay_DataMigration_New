using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Importer.Infrastructure.Sql
{
    public class SqlScriptExecutor : ISqlScriptExecutor
    {
        private readonly string _connectionString;
        private readonly ILogger<SqlScriptExecutor> _logger;

        public SqlScriptExecutor(IConfiguration config, ILogger<SqlScriptExecutor> logger)
        {
            _connectionString = config.GetConnectionString("db")
                ?? throw new InvalidOperationException("Connection string  not found.");
            _logger = logger;
        }

        public async Task ExecuteAsync(string sql, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(sql)) return;

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;

            _logger.LogInformation("Executing SQL script...");
            await cmd.ExecuteNonQueryAsync(ct);
            _logger.LogInformation("SQL script executed.");
        }

        public async Task ExecuteManyAsync(IEnumerable<string> scripts, CancellationToken ct = default)
        {
            foreach (var script in scripts)
            {
                await ExecuteAsync(script, ct);
            }
        }
    }

}
