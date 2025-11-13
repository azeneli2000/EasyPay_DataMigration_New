namespace Importer.Infrastructure.Configurations
{
    public class ImportConfig
    {
        public string ExcelPath { get; set; } = "";
        public string Mode { get; set; } = "";
        public TvpConfig? Tvp { get; set; }
        public BulkConfig? Bulk { get; set; }
    }

    public class TvpConfig
    {
        public string InsertSpName { get; set; } = "";
        public string ParameterName { get; set; } = "";
        public string TypeName { get; set; } = "";
        public string? ErrorSpName { get; set; }
        public string? ErrorParameterName { get; set; }
        public string? ErrorTypeName { get; set; }
    }

    public class BulkConfig
    {
        public string ValidTable { get; set; } = "";
        public string? ErrorTable { get; set; }
    }
}
