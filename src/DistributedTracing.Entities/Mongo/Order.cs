using System;

namespace DistributedTracing.Entities.Mongo
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
