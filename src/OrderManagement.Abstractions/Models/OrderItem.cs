namespace OrderManagement.Abstractions.Models
{
    using System;

    namespace OrderManagement.Abstractions.Models
    {
        public class OrderItem
        {
            public Guid Id { get; set; }
            public Guid OrderId { get; set; }
            public string ProductCode { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
        }
    }
}
