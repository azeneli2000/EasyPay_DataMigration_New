namespace Importer.Application.DTO
{
    public class ImportResult
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
    }
}
