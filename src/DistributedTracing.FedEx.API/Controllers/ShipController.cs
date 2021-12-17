using DistributedTracing.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DistributedTracing.FedEx.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShipController : ControllerBase
    {
        private readonly ILogger<ShipController> logger;

        public ShipController(ILogger<ShipController> logger)
        {
            this.logger = logger;
        }

        [Route("ship")]
        [HttpPost]
        public IActionResult Ship(Ship model)
        {
            logger.LogInformation("received shipping request, dispatching shipping now");
            return Ok();
        }
    }
}
