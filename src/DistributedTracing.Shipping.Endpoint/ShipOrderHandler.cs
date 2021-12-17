using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DistributedTracing.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace DistributedTracing.Shipping.Endpoint
{
    public class ShipOrderHandler : IHandleMessages<ShipOrder>
    {
        private readonly Func<HttpClient> httpClientFactory;
        private static readonly ILog Log = LogManager.GetLogger<ShippingSaga>();

        public ShipOrderHandler(Func<HttpClient> httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task Handle(ShipOrder message, IMessageHandlerContext context)
        {
            var httpClient = this.httpClientFactory();
            await httpClient.PostAsJsonAsync("/api/ship/ship", new Ship { OrderId = message.OrderId });
        }
    }
}
