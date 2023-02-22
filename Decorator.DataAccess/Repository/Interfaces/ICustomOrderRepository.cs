using Decorator.DataAccess.Models.DatabaseModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Decorator.DataAccess;

public interface ICustomOrderRepository
{
    Task<IEnumerable<CustomOrder>> GetAsync();

    Task<CustomOrder> GetAsync(int orderId);

    Task<IEnumerable<CustomOrder>> GetAsync(string search);

    Task<CustomOrder> UpsertAsync(CustomOrder order);

    Task DeleteAsync(int orderId);
}
