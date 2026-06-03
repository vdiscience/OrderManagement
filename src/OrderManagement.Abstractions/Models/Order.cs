namespace OrderManagement.Abstractions.Models
{
    using System;
    using System.Collections.Generic;

    namespace OrderManagement.Abstractions.Models
    {
        public class Order
        {
            public Guid Id { get; set; }
            public Guid CustomerId { get; set; }
            public string OrderNumber { get; set; }
            public decimal TotalAmount { get; set; }
            public OrderStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public List<OrderItem> OrderItems { get; set; } = new();
            public Address ShippingAddress { get; set; }
        }
    }
}
