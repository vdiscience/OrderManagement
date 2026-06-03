namespace OrderManagement.Abstractions.Models
{
    using global::OrderManagement.Abstractions.Models.OrderManagement.Abstractions.Models;
    using System;
    using System.Collections.Generic;

    namespace OrderManagement.Abstractions.Enums
    {
        public class Customer
        {
            public Guid Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public bool IsActive { get; set; }
            public List<Order> Orders { get; set; } = new();
        }
    }
}
