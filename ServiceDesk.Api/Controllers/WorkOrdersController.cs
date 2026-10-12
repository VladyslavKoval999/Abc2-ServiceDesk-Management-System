using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Shared.Entities;
using ServiceDesk.Api.Services;

namespace ServiceDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkOrdersController : ControllerBase
    {
        private readonly WorkOrderService _workOrderService;

        public WorkOrdersController(WorkOrderService workOrderService)
        {
            _workOrderService = workOrderService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_workOrderService.GetAllWorkOrders());
        }

        [HttpPost]
        public IActionResult Add([FromBody] WorkOrder workOrder)
        {
            _workOrderService.AddWorkOrder(workOrder);
            return Ok("Work order created successfully.");
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] WorkOrder workOrder)
        {
            if (id != workOrder.Id)
            {
                return BadRequest("ID mismatch");
            }

            _workOrderService.UpdateWorkOrder(workOrder);
            return Ok("Work order updated successfully.");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _workOrderService.DeleteWorkOrder(id);
            return Ok("Work order deleted successfully.");
        }
    }
}