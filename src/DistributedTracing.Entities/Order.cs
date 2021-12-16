using System;

namespace DistributedTracing.Entities
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
