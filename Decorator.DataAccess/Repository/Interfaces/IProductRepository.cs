using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Decorator.DataAccess;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAsync();

    Task<Product> GetAsync(int id);

    Task<IEnumerable<Product>> GetAsync(string search);
    Task<IEnumerable<ProductOrdersDTO>> GetProductOrdersAsync(int productId, DateTime dateFrom, DateTime dateTo);
    Task<IEnumerable<ProductDimension>> GetWithDimensionsAsync(string query);
    //Task<DeleteResult> RemoveProductDimensionAsync(int id);
    Task<Product> UpsertAsync(Product product);

}
