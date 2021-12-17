using System;
using NServiceBus;

namespace DistributedTracing.Messages
{
    public class ShipOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }
}
