using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Shared.Entities;
using ServiceDesk.Api.Services;

namespace ServiceDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerService _customerService;

        public CustomersController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_customerService.GetAllCustomers());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var entity = _customerService.GetCustomerById(id);
            if (entity == null) return NotFound("Customer not found.");
            return Ok(entity);
        }

        [HttpPost]
        public IActionResult Add([FromBody] Customer customer)
        {
            _customerService.AddCustomer(customer);
            return Ok("Customer created successfully.");
        }

        [HttpPut]
        public IActionResult Update([FromBody] Customer customer)
        {
            _customerService.UpdateCustomer(customer);
            return Ok("Customer updated successfully.");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _customerService.DeleteCustomer(id);
            return Ok("Customer deleted successfully.");
        }
    }
}