using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
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
            .ToListAsync();
    }

    public async Task<Product> GetAsync(int id)
    {
        return await _db.Products
            .Include(p => p.ProductDimensions)
            .FirstOrDefaultAsync(product => product.Id == id);
    }


    public async Task<IEnumerable<ProductOrdersDTO>> GetProductOrdersAsync(int productId, DateTime dateFrom, DateTime dateTo)
    {
        return await _db.OrderDetails
            .Where(x => x.ProductDimension.ProductId == productId)
            .Where(x => x.Order.PurchaseDate >= dateFrom && x.Order.PurchaseDate <= dateTo)
            .Select(x => new ProductOrdersDTO()
            {
                CustomerName = x.Order.CustomerName,
                InvoiceNumber = x.Order.InvoiceNumber,
                PurchaseDate = x.Order.PurchaseDate,
                Quantity = x.Quantity,
                Price = x.Price,
                DimensionX = x.ProductDimension.DimensionX,
                DimensionY = x.ProductDimension.DimensionY,
                ProductName = x.ProductDimension.Product.Name
            }).ToListAsync();
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
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
        if (existing == null)
        {
            _db.Products.Add(product);
        }
        else
        {
            // Load the dimensions for the existing product
            _db.Entry(existing).Collection(b => b.ProductDimensions).Load();

            // Get the list of the dimensions that were removed from the existing product
            var removedDimensions = existing.ProductDimensions.Exclude(product.ProductDimensions, i => i.Id).ToList();

            var existingDimensions = existing.ProductDimensions.Except(removedDimensions);


            foreach (var doc in removedDimensions)
            {
                // Remove the relationship between the dimensions and the product
                existing.ProductDimensions.Remove(doc);
            }

            // Get the list of the newly added dimensions
            var addedDimensions = product.ProductDimensions.Exclude(existing.ProductDimensions, i => i.Id).ToList();

            foreach (var pd in addedDimensions)
            {
                // The document exists in the repository, so we just attach it to the context
                _db.ProductDimensions.Attach(pd);

                // Create the relation between the batch and document
                existing.ProductDimensions.Add(pd);
            }

            foreach (var pd in existingDimensions)
            {
                int modfiedQuantity = product.ProductDimensions.FirstOrDefault(x => x.Id == pd.Id).Quantity;
                
                pd.Quantity = modfiedQuantity;
            }

            // Overwrite all property current values from modified product' entity values, 
            // so that it will have all modified values and mark entity as modified.
            _db.Entry(existing).CurrentValues.SetValues(product);
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