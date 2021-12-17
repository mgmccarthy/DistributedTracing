using System;
using NServiceBus;

namespace DistributedTracing.Messages
{
    public class OrderShipped : IEvent
    {
        public Guid OrderId { get; set; }
    }
}
