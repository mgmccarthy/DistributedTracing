using System;
using NServiceBus;

namespace DistributedTracing.Messages
{
    public class PlaceOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }
}
