using System.Threading.Tasks;
using DistributedTracing.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace DistributedTracing.Billing.Endpoint
{
    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        private static readonly ILog Log = LogManager.GetLogger<OrderPlacedHandler>();

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            Log.Info($"Handling OrderPlaced in Billing.Endpoint with OrderId: {message.OrderId}");
            return Task.CompletedTask;
        }
    }
}
