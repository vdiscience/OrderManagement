using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Abstractions.Grains;

public interface IOrderIndexGrain : Orleans.IGrainWithStringKey
{
    Task AddOrderToIndexAsync(Guid orderId, Guid customerId);
    Task RemoveOrderFromIndexAsync(Guid orderId);
    Task<List<Guid>> GetCustomerOrderIdsAsync(Guid customerId);
}
