using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Shared.Entities;
using ServiceDesk.Api.Services;

namespace ServiceDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasingController : ControllerBase
    {
        private readonly PurchasingService _purchasingService;

        public PurchasingController(PurchasingService purchasingService)
        {
            _purchasingService = purchasingService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_purchasingService.GetAllPurchases());
        }

        [HttpPost]
        public IActionResult Record([FromBody] Purchase purchase)
        {
            _purchasingService.RecordPurchase(purchase);
            return Ok("Purchase recorded successfully.");
        }
    }
}