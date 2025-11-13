using Importer.Infrastructure.Configurations;

namespace Importer.Infrastructure.Sql.Scripts
{
    public static class BootstrapScriptFactory
    {

        public static IEnumerable<string> FromConfigs(Dictionary<string, ImportConfig> configs)
        {
            var all = new List<string>();

            foreach (var (name, cfg) in configs)
            {
                var scriptsForThisImport = FromConfig(cfg, name);
                all.AddRange(scriptsForThisImport);
            }

            return all;
        }

        public static IEnumerable<string> FromConfig(ImportConfig config, string? importName = null)
        {
            var scripts = new List<string>();

     
            if (config.Bulk != null && !string.IsNullOrWhiteSpace(config.Bulk.ErrorTable))
            {
                var errorTableName = config.Bulk.ErrorTable;
                var errorTableShort = errorTableName.Contains('.')
                    ? errorTableName.Split('.').Last()
                    : errorTableName;

                scripts.Add($@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'dbo' AND t.name = '{errorTableShort}'
)
BEGIN
   CREATE TABLE {errorTableName}
    (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        TechnicianName  NVARCHAR(200) NULL,
        ClientName      NVARCHAR(200) NULL,
        [Date]          DATETIME2 NULL,
        Total           DECIMAL(18,2) NULL,
        Reason          NVARCHAR(500) NULL,
        OriginalNotes   NVARCHAR(MAX) NULL,
        CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
END;
");
            }

            if (config.Mode.Equals("Tvp", StringComparison.OrdinalIgnoreCase) && config.Tvp != null)
            {
                var tvpShortName = config.Tvp.TypeName.Split('.').Last();

                scripts.Add($@"
IF NOT EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = '{tvpShortName}')
BEGIN
    CREATE TYPE {config.Tvp.TypeName} AS TABLE
    (
        TechnicianId INT NOT NULL,
        ClientId INT NOT NULL,
        Information NVARCHAR(MAX) NULL,
        [Date] DATE NOT NULL,
        Total DECIMAL(18,2) NOT NULL
    );
END;
");

                if (!string.IsNullOrWhiteSpace(config.Tvp.InsertSpName))
                {
                    var tvpParam = config.Tvp.ParameterName;

                    scripts.Add($@"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{config.Tvp.InsertSpName}') AND type IN (N'P', N'PC'))
BEGIN
    EXEC('
    CREATE PROCEDURE {config.Tvp.InsertSpName}
        {tvpParam} {config.Tvp.TypeName} READONLY,
        @PRESULT INT OUTPUT
    AS
    BEGIN
        SET NOCOUNT ON;

        INSERT INTO dbo.WorkOrders (TechnicianId, ClientId, Information, [Date], Total)
        SELECT TechnicianId, ClientId, Information, [Date], Total
        FROM {tvpParam};

        SET @PRESULT = @@ROWCOUNT;
    END
    ');
END;
");
                }

                if (!string.IsNullOrWhiteSpace(config.Tvp.ErrorSpName) &&
                    !string.IsNullOrWhiteSpace(config.Tvp.ErrorParameterName) &&
                    !string.IsNullOrWhiteSpace(config.Tvp.ErrorTypeName))
                {
                    var errParam = config.Tvp.ErrorParameterName;
                    var errTypeShort = config.Tvp.ErrorTypeName.Split('.').Last();

                    scripts.Add($@"
IF NOT EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = '{errTypeShort}')
BEGIN
    CREATE TYPE {config.Tvp.ErrorTypeName} AS TABLE
    (
       TechnicianName  NVARCHAR(200) NULL,
        ClientName      NVARCHAR(200) NULL,
        [Date]          DATETIME2 NULL,
        Total           DECIMAL(18,2) NULL,
        Reason          NVARCHAR(500) NULL,
        OriginalNotes   NVARCHAR(MAX) NULL
    );
END;
");

                    scripts.Add($@"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{config.Tvp.ErrorSpName}') AND type IN (N'P', N'PC'))
BEGIN
    EXEC('
    CREATE PROCEDURE {config.Tvp.ErrorSpName}
        {errParam} {config.Tvp.ErrorTypeName} READONLY,
        @PRESULT INT OUTPUT
    AS
    BEGIN
        SET NOCOUNT ON;

        INSERT INTO dbo.LoadingErrors
            (TechnicianName, ClientName, [Date], Total, Reason, OriginalNotes)
        SELECT
            TechnicianName,
            ClientName,
            [Date],
            Total,
            Reason,
            OriginalNotes
        FROM {errParam};

        SET @PRESULT = @@ROWCOUNT;
    END
    ');
END;
");
                }
            }

            return scripts;
        }
    }

}
