using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Decorator.DataAccess;

namespace Decorator.DataAccess;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAsync();

    Task<Product> GetAsync(Guid id);

    Task<IEnumerable<Product>> GetAsync(string search);

    Task<Product> UpsertAsync(Product product);

}
