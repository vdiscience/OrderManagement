using OrderManagement.Abstractions.Enums;
using OrderManagement.Abstractions.Models.OrderManagement.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Abstractions.Grains
{
    public interface IOrderGrain : IGrainWithGuidKey
    {
        Task<Order> GetOrderAsync();
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> UpdateOrderStatusAsync(OrderStatus newStatus);
        Task<Order> UpdateOrderAsync(Order order);
        Task CancelOrderAsync(string reason);
    }
}
