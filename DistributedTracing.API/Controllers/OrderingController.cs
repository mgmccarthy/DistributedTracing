using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DistributedTracing.Messages;
using NServiceBus;

namespace DistributedTracing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderingController : ControllerBase
    {
        private readonly IMessageSession messageSession;

        public OrderingController(IMessageSession messageSession)
        {
            this.messageSession = messageSession;
        }

        [HttpGet]
        public async Task<ActionResult<Guid>> Get(string message)
        {
            var command = new PlaceOrder { OrderId = Guid.NewGuid() };
            
            Activity.Current?.AddBaggage("order.id", command.OrderId.ToString());
           
            await messageSession.Send(command);
            
            return Accepted(command.OrderId);
        }
    }
}
