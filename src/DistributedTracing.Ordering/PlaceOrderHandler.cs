using System.Diagnostics;
using System.Threading.Tasks;
using DistributedTracing.Entities;
using DistributedTracing.Messages;
using MongoDB.Driver;
using NServiceBus;
using NServiceBus.Logging;

namespace DistributedTracing.Ordering.Endpoint
{
    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        private readonly IMongoClient mongoClient;
        private static readonly ILog Log = LogManager.GetLogger<PlaceOrderHandler>();

        public PlaceOrderHandler(IMongoClient mongoClient)
        {
            this.mongoClient = mongoClient;
        }

        public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            Log.Info($"Handling PlaceOrder with OrderId: {message.OrderId}");

            //can add baggage like this
            Activity.Current?.AddBaggage("AddBaggageKey_fromPlaceOrderHandler", "AddBaggageValue_fromPlaceOrderHandler");

            var database = mongoClient.GetDatabase("DistributedTracing");
            var collection = database.GetCollection<Order>("orders");
            var order = new Order { OrderId = message.OrderId };
            await collection.InsertOneAsync(order);

            await context.Publish(new OrderPlaced {OrderId = message.OrderId});
        }
    }
}
