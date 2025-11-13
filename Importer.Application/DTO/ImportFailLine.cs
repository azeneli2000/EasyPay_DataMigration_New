namespace Importer.Application.DTO
{
    public class ImportFailLine
    {
        public string? Technician { get; set; } = "";
        public string? Client { get; set; } = "";
        public DateTime? Date { get; set; }
        public decimal Total { get; set; } = 0;

        public string? Notes { get; set; } = "";
        public string? Reason { get; set; } = "";

    }
}
