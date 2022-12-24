using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Product> GetAsync(Guid id)
    {
        return await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id);
    }

    public async Task<Product> UpsertAsync(Product product)
    {
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
        if (existing == null)
        {
            _db.Products.Add(product);
        }
        else
        {
            _db.Entry(existing).CurrentValues.SetValues(product);
        }
        await _db.SaveChangesAsync();
        return product;
    }


    public async Task<IEnumerable<Product>> GetAsync(string value)
    {
        return await _db.Products.Where(product =>
            product.Code.StartsWith(value) 
            || product.Name.StartsWith(value))
        .AsNoTracking()
        .ToListAsync();
    }
}
