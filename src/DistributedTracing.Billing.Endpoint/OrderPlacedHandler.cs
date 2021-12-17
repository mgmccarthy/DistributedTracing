using System.Threading.Tasks;
using DistributedTracing.Entities.SqlServer;
using DistributedTracing.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace DistributedTracing.Billing.Endpoint
{
    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        private readonly OrderContext dbContext;
        private static readonly ILog Log = LogManager.GetLogger<OrderPlacedHandler>();

        public OrderPlacedHandler(OrderContext dbContext)
        {
            this.dbContext = dbContext;
            //DbInitializer.Initialize(this.dbContext);
        }

        public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            Log.Info($"Handling OrderPlaced in Billing.Endpoint with OrderId: {message.OrderId}");

            await dbContext.Orders.AddAsync(new Order { OrderId = message.OrderId });
            await dbContext.SaveChangesAsync();

            await context.Publish(new OrderBilled { OrderId = message.OrderId });
        }
    }
}
