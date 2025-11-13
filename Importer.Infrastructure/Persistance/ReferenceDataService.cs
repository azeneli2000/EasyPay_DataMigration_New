using Importer.Application.Interfaces;
using Importer.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Importer.Infrastructure.Persistance
{

    public class ReferenceDataService : IReferenceDataService
    {
        private readonly string _conn;
        public ReferenceDataService(IConfiguration configuration)
        {
            _conn = configuration.GetConnectionString("db")!;
        }
        public async Task<List<Client>> LoadClientsAsync()
        {
            var list = new List<Client>();
            const string sql = "SELECT ClientId, FirstName, LastName FROM dbo.Clients";

            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new Client(
                    rdr.GetInt32(0),
                    rdr.GetString(1),
                    rdr.GetString(2)
                ));
            }
            return list;
        }

        public async  Task<Dictionary<string, int>> LoadTechniciansAsync()
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            const string sql = "SELECT TechnicianId, FirstName, LastName FROM dbo.Technicians";

            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                var id = rdr.GetInt32(0);
                var fn = rdr.GetString(1);
                var ln = rdr.GetString(2);
                var full = (fn + " " + ln).Trim();
                if (!dict.ContainsKey(full))
                    dict.Add(full, id);
            }
            return dict;
        }
    }
}
