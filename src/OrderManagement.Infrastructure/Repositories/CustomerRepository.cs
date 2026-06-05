using Dapper;
using OrderManagement.Abstractions.Models.OrderManagement.Abstractions.Enums;
using OrderManagement.Abstractions.Models.OrderManagement.Abstractions.Models;
using OrderManagement.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly DapperContext _context;

    public CustomerRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        using var connection = _context.CreateConnection();

        const string customerSql = @"
                SELECT * FROM Customers WHERE Id = @Id AND IsActive = 1";

        var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
            customerSql, new { Id = id });

        if (customer != null)
        {
            const string ordersSql = @"
                    SELECT * FROM Orders WHERE CustomerId = @CustomerId";

            customer.Orders = (await connection.QueryAsync<Order>(
                ordersSql, new { CustomerId = id })).ToList();

            foreach (var order in customer.Orders)
            {
                const string itemsSql = @"
                        SELECT * FROM OrderItems WHERE OrderId = @OrderId";

                order.OrderItems = (await connection.QueryAsync<OrderItem>(
                    itemsSql, new { OrderId = order.Id })).ToList();
            }
        }

        return customer;
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        using var connection = _context.CreateConnection();

        const string sql = @"
                INSERT INTO Customers (Id, FirstName, LastName, Email, PhoneNumber, CreatedAt, IsActive)
                VALUES (@Id, @FirstName, @LastName, @Email, @PhoneNumber, @CreatedAt, @IsActive);
                
                SELECT * FROM Customers WHERE Id = @Id";

        customer.Id = customer.Id == Guid.Empty ? Guid.NewGuid() : customer.Id;
        customer.CreatedAt = DateTime.UtcNow;
        customer.IsActive = true;

        return await connection.QuerySingleAsync<Customer>(sql, customer);
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        using var connection = _context.CreateConnection();

        const string sql = @"
                UPDATE Customers 
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    PhoneNumber = @PhoneNumber,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND IsActive = 1;
                
                SELECT * FROM Customers WHERE Id = @Id";

        customer.UpdatedAt = DateTime.UtcNow;

        return await connection.QuerySingleAsync<Customer>(sql, customer);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _context.CreateConnection();

        const string sql = @"
                UPDATE Customers 
                SET IsActive = 0, UpdatedAt = @UpdatedAt 
                WHERE Id = @Id";

        await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();

        const string sql = "SELECT * FROM Customers WHERE IsActive = 1";

        return await connection.QueryAsync<Customer>(sql);
    }
}
