using System;
using NServiceBus;

namespace DistributedTracing.Messages
{
    public class OrderBilled : IEvent
    {
        public Guid OrderId { get; set; }
    }
}
