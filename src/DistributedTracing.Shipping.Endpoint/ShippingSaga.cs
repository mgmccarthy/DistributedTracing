using System;
using System.Threading.Tasks;
using DistributedTracing.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace DistributedTracing.Shipping.Endpoint
{
    public class ShippingSaga : Saga<ShippingSaga.SagaData>,
        IAmStartedByMessages<OrderBilled>,
        IAmStartedByMessages<OrderPlaced>
    {
        private static readonly ILog Log = LogManager.GetLogger<ShippingSaga>();

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaData> mapper)
        {
            mapper.ConfigureMapping<OrderBilled>(message => message.OrderId).ToSaga(sagaData => sagaData.OrderId);
            mapper.ConfigureMapping<OrderPlaced>(message => message.OrderId).ToSaga(sagaData => sagaData.OrderId);
        }

        public async Task Handle(OrderBilled message, IMessageHandlerContext context)
        {
            Log.Info("Handling OrderBilled in ShippingSaga");
            Data.OrderBilled = true;
            await CheckForCompletionAndSendShipOrder(context);
        }

        public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            Log.Info("Handling OrderPlaced in ShippingSaga");
            Data.OrderPlaced = true; 
            await CheckForCompletionAndSendShipOrder(context);
        }

        public async Task CheckForCompletionAndSendShipOrder(IMessageHandlerContext context)
        {
            if (Data.OrderBilled && Data.OrderPlaced)
            {
                Log.Info("Sending ShipOrder from ShippingSaga");
                await context.SendLocal(new ShipOrder {OrderId = Data.OrderId});
                MarkAsComplete();
            }
        }

        public class SagaData : ContainSagaData
        {
            public Guid OrderId { get; set; }
            public bool OrderBilled { get; set; }
            public bool OrderPlaced { get; set; }
        }

    }
}
