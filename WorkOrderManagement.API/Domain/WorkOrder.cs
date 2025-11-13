namespace WorkOrderManagement.API.Domain
{
    public class WorkOrder
    {
        public int WorkOrderId { get; set; }
        public int TechnicianId { get; set; }
        public int ClientId { get; set; }
        public string Information { get; set; } = "";
        public DateTime Date { get; set; }
        public decimal Total { get; set; }

        public Technician? Technician { get; set; }
        public Client? Client { get; set; }
    }
}
