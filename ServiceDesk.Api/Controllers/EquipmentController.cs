using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Shared.Entities;
using ServiceDesk.Api.Services;

namespace ServiceDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private readonly EquipmentService _equipmentService;

        public EquipmentController(EquipmentService equipmentService)
        {
            _equipmentService = equipmentService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_equipmentService.GetAllEquipment());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var entity = _equipmentService.GetEquipmentById(id);
            if (entity == null) return NotFound("Equipment not found.");
            return Ok(entity);
        }

        [HttpPost]
        public IActionResult Add([FromBody] Equipment equipment)
        {
            _equipmentService.AddEquipment(equipment);
            return Ok("Equipment added successfully.");
        }

        [HttpPut]
        public IActionResult Update([FromBody] Equipment equipment)
        {
            _equipmentService.UpdateEquipment(equipment);
            return Ok("Equipment updated successfully.");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _equipmentService.DeleteEquipment(id);
            return Ok("Equipment deleted successfully.");
        }
    }
}