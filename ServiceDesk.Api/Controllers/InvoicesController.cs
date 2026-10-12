using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Shared.Entities;
using ServiceDesk.Api.Services;

namespace ServiceDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly InvoiceService _invoiceService;

        public InvoicesController(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_invoiceService.GetAllInvoices());
        }

        [HttpPost]
        public IActionResult Create([FromBody] Invoice invoice)
        {
            _invoiceService.CreateInvoice(invoice);
            return Ok("Invoice created successfully.");
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] string newStatus)
        {
            _invoiceService.UpdateStatus(id, newStatus);
            return Ok("Invoice status updated successfully.");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _invoiceService.DeleteInvoice(id);
            return Ok("Invoice deleted successfully.");
        }
    }
}