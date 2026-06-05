using Dapper;
using OrderManagement.Abstractions.Enums;
using OrderManagement.Abstractions.Models.OrderManagement.Abstractions.Models;
using OrderManagement.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DapperContext _context;

        public OrderRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Order> GetByIdAsync(Guid id)
        {
            using var connection = _context.CreateConnection();

            const string orderSql = "SELECT * FROM Orders WHERE Id = @Id";
            var order = await connection.QuerySingleOrDefaultAsync<Order>(orderSql, new { Id = id });

            if (order == null)
                throw new KeyNotFoundException($"Order with id {id} not found");

            const string itemsSql = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";
            order.OrderItems = (await connection.QueryAsync<OrderItem>(itemsSql, new { OrderId = id })).ToList();

            return order;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            using var connection = _context.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                const string orderSql = @"
                    INSERT INTO Orders (Id, CustomerId, OrderNumber, TotalAmount, Status, CreatedAt, ShippingAddress)
                    VALUES (@Id, @CustomerId, @OrderNumber, @TotalAmount, @Status, @CreatedAt, @ShippingAddress);
                    
                    SELECT * FROM Orders WHERE Id = @Id";

                order.Id = order.Id == Guid.Empty ? Guid.NewGuid() : order.Id;
                order.OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{order.Id.ToString().Substring(0, 8).ToUpper()}";
                order.CreatedAt = DateTime.UtcNow;
                order.Status = OrderStatus.Pending;

                var createdOrder = await connection.QuerySingleAsync<Order>(orderSql, order, transaction);

                createdOrder.OrderItems = new List<OrderItem>();

                if (order.OrderItems?.Any() == true)
                {
                    foreach (var item in order.OrderItems)
                    {
                        item.Id = Guid.NewGuid();
                        item.OrderId = createdOrder.Id;
                        item.TotalPrice = item.Quantity * item.UnitPrice;

                        await connection.ExecuteAsync(@"
                            INSERT INTO OrderItems (Id, OrderId, ProductCode, ProductName, Quantity, UnitPrice, TotalPrice)
                            VALUES (@Id, @OrderId, @ProductCode, @ProductName, @Quantity, @UnitPrice, @TotalPrice)",
                            item, transaction);

                        createdOrder.OrderItems.Add(item);
                    }
                }

                transaction.Commit();
                return createdOrder;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            using var connection = _context.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                const string orderSql = @"
                    UPDATE Orders 
                    SET TotalAmount = @TotalAmount,
                        UpdatedAt = @UpdatedAt,
                        ShippingAddress = @ShippingAddress
                    WHERE Id = @Id;
                    
                    SELECT * FROM Orders WHERE Id = @Id";

                order.UpdatedAt = DateTime.UtcNow;
                var updatedOrder = await connection.QuerySingleAsync<Order>(orderSql, order, transaction);

                // Update order items
                await connection.ExecuteAsync("DELETE FROM OrderItems WHERE OrderId = @OrderId",
                    new { OrderId = order.Id }, transaction);

                var insertedItems = new List<OrderItem>();

                if (order.OrderItems?.Any() == true)
                {
                    foreach (var item in order.OrderItems)
                    {
                        item.Id = Guid.NewGuid();
                        item.OrderId = order.Id;
                        item.TotalPrice = item.Quantity * item.UnitPrice;

                        await connection.ExecuteAsync(@"
                            INSERT INTO OrderItems (Id, OrderId, ProductCode, ProductName, Quantity, UnitPrice, TotalPrice)
                            VALUES (@Id, @OrderId, @ProductCode, @ProductName, @Quantity, @UnitPrice, @TotalPrice)",
                            item, transaction);

                        insertedItems.Add(item);
                    }
                }

                updatedOrder.OrderItems = insertedItems;
                transaction.Commit();
                return updatedOrder;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Order> UpdateStatusAsync(Guid id, OrderStatus status)
        {
            using var connection = _context.CreateConnection();

            const string sql = @"
                UPDATE Orders 
                SET Status = @Status, UpdatedAt = @UpdatedAt 
                WHERE Id = @Id;
                
                SELECT * FROM Orders WHERE Id = @Id";

            var order = await connection.QuerySingleAsync<Order>(sql,
                new { Id = id, Status = status, UpdatedAt = DateTime.UtcNow });

            if (order != null)
            {
                const string itemsSql = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";
                order.OrderItems = (await connection.QueryAsync<OrderItem>(itemsSql, new { OrderId = id })).ToList();
            }

            return order;
        }

        public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(Guid customerId)
        {
            using var connection = _context.CreateConnection();

            const string sql = "SELECT * FROM Orders WHERE CustomerId = @CustomerId";
            var orders = await connection.QueryAsync<Order>(sql, new { CustomerId = customerId });

            foreach (var order in orders)
            {
                const string itemsSql = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";
                order.OrderItems = (await connection.QueryAsync<OrderItem>(itemsSql, new { OrderId = order.Id })).ToList();
            }

            return orders;
        }

        public async Task AddOrderItemAsync(OrderItem item)
        {
            using var connection = _context.CreateConnection();

            item.Id = Guid.NewGuid();
            item.TotalPrice = item.Quantity * item.UnitPrice;

            const string sql = @"
                INSERT INTO OrderItems (Id, OrderId, ProductCode, ProductName, Quantity, UnitPrice, TotalPrice)
                VALUES (@Id, @OrderId, @ProductCode, @ProductName, @Quantity, @UnitPrice, @TotalPrice)";

            await connection.ExecuteAsync(sql, item);
        }

        public async Task UpdateOrderItemsAsync(Guid orderId, List<OrderItem> items)
        {
            using var connection = _context.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync("DELETE FROM OrderItems WHERE OrderId = @OrderId",
                    new { OrderId = orderId }, transaction);

                foreach (var item in items)
                {
                    item.Id = Guid.NewGuid();
                    item.OrderId = orderId;
                    item.TotalPrice = item.Quantity * item.UnitPrice;

                    await connection.ExecuteAsync(@"
                        INSERT INTO OrderItems (Id, OrderId, ProductCode, ProductName, Quantity, UnitPrice, TotalPrice)
                        VALUES (@Id, @OrderId, @ProductCode, @ProductName, @Quantity, @UnitPrice, @TotalPrice)",
                        item, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
