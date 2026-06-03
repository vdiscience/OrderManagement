using OrderManagement.Abstractions.Models.OrderManagement.Abstractions.Enums;
using OrderManagement.Abstractions.Models.OrderManagement.Abstractions.Models;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Abstractions.Grains
{
    public interface ICustomerGrain : IGrainWithGuidKey
    {
        Task<Customer> GetCustomerAsync();
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task DeactivateCustomerAsync();
        Task<List<Order>> GetCustomerOrdersAsync();
    }
}
