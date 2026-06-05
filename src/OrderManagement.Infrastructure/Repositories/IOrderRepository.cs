using OrderManagement.Abstractions.Enums;
using OrderManagement.Abstractions.Models.OrderManagement.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Infrastructure.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(Guid id);
        Task<Order> CreateAsync(Order order);
        Task<Order> UpdateAsync(Order order);
        Task<Order> UpdateStatusAsync(Guid id, OrderStatus status);
        Task<IEnumerable<Order>> GetCustomerOrdersAsync(Guid customerId);
        Task AddOrderItemAsync(OrderItem item);
        Task UpdateOrderItemsAsync(Guid orderId, List<OrderItem> items);
    }
}
