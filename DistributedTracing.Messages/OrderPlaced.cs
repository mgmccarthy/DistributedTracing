using System;
using NServiceBus;

namespace DistributedTracing.Messages
{
    public class OrderPlaced : IEvent
    {
        public Guid OrderId { get; set; }
    }
}
