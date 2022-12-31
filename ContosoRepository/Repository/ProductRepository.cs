using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Decorator.DataAccess;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db)
    {
        _db = db; 
    }

    public async Task<IEnumerable<Product>> GetAsync()
    {
        return await _db.Products
            .Include(p=> p.ProductDimensions)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Product> GetAsync(int id)
    {
        return await _db.Products
            .Include(p => p.ProductDimensions)
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id);
    }


    /// <summary>
    /// Search products by query and include Name and tis dimensions
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ProductDimension>> GetWithDimensionsAsync(string query)
    {
        return await _db.ProductDimensions
                          .Include(x=> x.Product)
                          .Where(pd => pd.Product.Code.StartsWith(query) || pd.Product.Name.StartsWith(query))
                          .OrderBy(x=> x.Product.Code)
                          .ToListAsync();
    }

    public async Task<Product> UpsertAsync(Product product)
    {
        var existing = await _db.Products.Include(x=> x.ProductDimensions).FirstOrDefaultAsync(p => p.Id == product.Id);
        if (existing == null)
        {
            _db.Products.Add(product);
        }
        else
        {
            existing.Code = product.Code;
            existing.Name = product.Name;

            //Update product dimensions
            existing.ProductDimensions.Clear();
            foreach(var pd in product.ProductDimensions)
                existing.ProductDimensions.Add(pd);
            
            _db.Products.Update(existing);
        }
        await _db.SaveChangesAsync();
        return existing;
    }


    public async Task<IEnumerable<Product>> GetAsync(string value)
    {
        return await _db.Products.Where(product =>
            product.Code.StartsWith(value) 
            || product.Name.StartsWith(value))
         .Include(p => p.ProductDimensions)
        .AsNoTracking()
        .ToListAsync();
    }

    //public async Task<DeleteResult> RemoveProductDimensionAsync(int id)
    //{
    //    var match = await _db.ProductDimensions.FindAsync(id);
    //    if (match == null)
    //    {
    //        return DeleteResult.NotExist;
    //    }

    //    var isproductDimensionInOrder = _db.OrderDetails.Any(x => x.ProductDimensionId == id);
    //    if (isproductDimensionInOrder)
    //    {
    //        return DeleteResult.InOrder;
    //    }

    //    _db.ProductDimensions.Remove(match);
    //    await _db.SaveChangesAsync();

    //    return DeleteResult.Success;
    //}

}

