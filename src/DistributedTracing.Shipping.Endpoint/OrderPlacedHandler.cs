using System.Threading.Tasks;
using DistributedTracing.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace DistributedTracing.Shipping.Endpoint
{
    //public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    //{
    //    private static readonly ILog Log = LogManager.GetLogger<OrderPlacedHandler>();

    //    public Task Handle(OrderPlaced message, IMessageHandlerContext context)
    //    {
    //        Log.Info($"Handling OrderPlaced in Shipping.Endpoint with OrderId: {message.OrderId}");
    //        return Task.CompletedTask;
    //    }
    //}
}
