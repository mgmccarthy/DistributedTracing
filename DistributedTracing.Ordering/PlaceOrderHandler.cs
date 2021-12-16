using System.Threading.Tasks;
using DistributedTracing.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace DistributedTracing.Ordering.Endpoint
{
    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        private static readonly ILog Log = LogManager.GetLogger<PlaceOrderHandler>();

        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            Log.Info($"Handling PlaceOrder with OrderId: {message.OrderId}");
            return context.Publish(new OrderPlaced {OrderId = message.OrderId});
        }
    }
}
