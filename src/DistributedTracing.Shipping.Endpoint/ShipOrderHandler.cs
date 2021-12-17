using System;
using System.Threading.Tasks;
using DistributedTracing.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace DistributedTracing.Shipping.Endpoint
{
    public class ShipOrderHandler : IHandleMessages<ShipOrder>
    {
        private static readonly ILog Log = LogManager.GetLogger<ShippingSaga>();

        public Task Handle(ShipOrder message, IMessageHandlerContext context)
        {
            throw new NotImplementedException();
        }
    }
}
