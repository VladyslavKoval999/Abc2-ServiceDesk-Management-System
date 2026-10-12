using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Shared.Entities;
using ServiceDesk.Api.Services;
using System;

namespace ServiceDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotationsController : ControllerBase
    {
        private readonly QuotationService _quotationService;

        public QuotationsController(QuotationService quotationService)
        {
            _quotationService = quotationService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_quotationService.GetAllQuotations());
        }

        [HttpPost]
        public IActionResult Add([FromBody] Quotation quotation)
        {
            _quotationService.AddQuotation(quotation);
            return Ok("Quotation created successfully.");
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Quotation quotation)
        {
            if (id != quotation.Id)
                return BadRequest("ID mismatch.");

            try
            {
                _quotationService.UpdateQuotation(quotation);
                return Ok("Quotation updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] string newStatus)
        {
            _quotationService.UpdateStatus(id, newStatus);
            return Ok("Quotation status updated successfully.");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _quotationService.DeleteQuotation(id);
            return Ok("Quotation deleted successfully.");
        }
    }
}