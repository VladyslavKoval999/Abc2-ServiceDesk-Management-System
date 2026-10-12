using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Shared.Entities;
using ServiceDesk.Api.Services;

namespace ServiceDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly SupplierService _supplierService;

        public SuppliersController(SupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_supplierService.GetAllSuppliers());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var entity = _supplierService.GetSupplierById(id);
            if (entity == null) return NotFound("Supplier not found.");
            return Ok(entity);
        }

        [HttpPost]
        public IActionResult Add([FromBody] Supplier supplier)
        {
            _supplierService.AddSupplier(supplier);
            return Ok("Supplier added successfully.");
        }

        [HttpPut]
        public IActionResult Update([FromBody] Supplier supplier)
        {
            _supplierService.UpdateSupplier(supplier);
            return Ok("Supplier updated successfully.");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _supplierService.DeleteSupplier(id);
            return Ok("Supplier deleted successfully.");
        }
    }
}