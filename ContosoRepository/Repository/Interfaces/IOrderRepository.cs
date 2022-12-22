using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Decorator.DataAccess.Models;

namespace Decorator.DataAccess;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAsync();

    Task<Order> GetAsync(Guid orderId);

    Task<IEnumerable<Order>> GetAsync(string search);

    Task<IEnumerable<Order>> GetForCustomerAsync(Guid customerId);

    Task<Order> UpsertAsync(Order order);

    Task DeleteAsync(Guid orderId);
}
