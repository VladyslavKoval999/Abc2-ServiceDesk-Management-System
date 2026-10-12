using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Shared.Entities;
using ServiceDesk.Api.Services;

namespace ServiceDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public InventoryController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_inventoryService.GetAllItems());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var entity = _inventoryService.GetItemById(id);
            if (entity == null) return NotFound("Inventory item not found.");
            return Ok(entity);
        }

        [HttpPost]
        public IActionResult Add([FromBody] InventoryItem item)
        {
            _inventoryService.AddItem(item);
            return Ok("Inventory item added successfully.");
        }

        [HttpPut]
        public IActionResult Update([FromBody] InventoryItem item)
        {
            _inventoryService.UpdateItem(item);
            return Ok("Inventory item updated successfully.");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _inventoryService.DeleteItem(id);
            return Ok("Inventory item deleted successfully.");
        }
    }
}