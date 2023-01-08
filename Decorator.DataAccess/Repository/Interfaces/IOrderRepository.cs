using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Decorator.DataAccess;

namespace Decorator.DataAccess;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAsync();

    Task<Order> GetAsync(int orderId);

    Task<IEnumerable<Order>> GetAsync(string search);

    Task<Order> UpsertAsync(Order order);

    Task DeleteAsync(int orderId);
}