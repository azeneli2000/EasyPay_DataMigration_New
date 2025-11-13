namespace Importer.Application.DTO
{
    public class ImportSuccessLine
    {
        public string Technician { get; set; } = "";
        public string Client { get; set; } = "";
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
    }
}
