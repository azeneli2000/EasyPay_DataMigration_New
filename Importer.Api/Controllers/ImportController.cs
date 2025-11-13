using Importer.Application.Interfaces;
using Importer.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Importer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly IWorkOrderImportService _workOrderImportService;
        private readonly IClientImportService _clientImportService;
        private readonly ITechnicianImportService _technicianImportService;
        private readonly IConfiguration _config;

        public ImportController(IWorkOrderImportService importService, IConfiguration config, IClientImportService clientImportService, ITechnicianImportService technicianImportService)
        {
            _config = config;
            _workOrderImportService = importService;
            _clientImportService = clientImportService;
            _technicianImportService = technicianImportService;
        }

        [HttpPost("workorders")]
        public async Task<IActionResult> RunWorkOrders(CancellationToken ct)
        {
            var result = await _workOrderImportService.ImportAsync(DataLoaderName.WorkOrdersFromExcel, ct);
            return Ok(result);
        }

        [HttpPost("clients")]
        public async Task<IActionResult> RunClients(CancellationToken ct)
        {
            await _clientImportService.ImportClientsAsync(DataLoaderName.ClientsFromExcel, ct);
            return Ok();
        }

        [HttpPost("technicians")]
        public async Task<IActionResult> RunTechnician(CancellationToken ct)
        {
            await _technicianImportService.ImportTechniciansAsync(DataLoaderName.TechniciansFromExcel, ct);
            return Ok();
        }
    }
}
