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
}
