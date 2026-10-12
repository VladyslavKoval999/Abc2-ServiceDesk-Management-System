using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Api.Services;
using ServiceDesk.Shared.Entities;
using System;

namespace ServiceDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TechniciansController : ControllerBase
    {
        private readonly TechnicianService _technicianService;

        public TechniciansController(TechnicianService technicianService)
        {
            _technicianService = technicianService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_technicianService.GetAllTechnicians());
        }

        [HttpPost]
        public IActionResult Add([FromBody] Technician technician)
        {
            _technicianService.AddTechnician(technician);
            return Ok("Technician added successfully.");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTechnician(int id)
        {
            try
            {
                bool isDeleted = _technicianService.DeleteTechnician(id);

                if (!isDeleted)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}