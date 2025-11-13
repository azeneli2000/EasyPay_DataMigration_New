using Importer.Application.Interfaces;
using Importer.Domain.Enums;
using Importer.Infrastructure.Configurations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Importer.Infrastructure.Persistance
{
    public class SpTvpImportExecutor : IDbImportExecutor
    {
        private readonly IConfiguration _config;

        public SpTvpImportExecutor(IConfiguration config)
        {
            _config = config;
        }

        public async Task ExecuteAsync( DataTable table, bool valid)
        {
            if (table == null || table.Rows.Count == 0)
                return;

            var connectionString = _config.GetConnectionString();
          
            var importConfig = _config.GetImportConfiguration(DataLoaderName.WorkOrdersFromExcel);
          
            string? spName;
            string? paramName;
            string? typeName;

            if (valid)
            {
                spName = importConfig.Tvp.InsertSpName;
                paramName = importConfig.Tvp.ParameterName;
                typeName = importConfig.Tvp.TypeName;
            }
            else
            {
                spName = importConfig.Tvp.ErrorSpName;
                paramName = importConfig.Tvp.ErrorParameterName;
                typeName = importConfig.Tvp.ErrorTypeName;
            }

            if (string.IsNullOrWhiteSpace(spName) ||
                string.IsNullOrWhiteSpace(paramName) ||
                string.IsNullOrWhiteSpace(typeName))
                throw new InvalidOperationException($"TVP parameters not fully configured for '{DataLoaderName.WorkOrdersFromExcel}' (valid={valid}).");

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(spName, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            var output = new SqlParameter("@PRESULT", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(output);

            var tvp = new SqlParameter(paramName, SqlDbType.Structured)
            {
                TypeName = typeName,
                Value = table
            };
            cmd.Parameters.Add(tvp);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
