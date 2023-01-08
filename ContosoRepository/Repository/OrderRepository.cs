using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Decorator.DataAccess
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _db; 

        public OrderRepository(ApplicationDbContext db) => _db = db;

        public async Task<IEnumerable<Order>> GetAsync() =>
            await _db.Orders
                .Include(order => order.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.ProductDimension)
                .ThenInclude(pd => pd.Product)
                .AsNoTracking()
                .ToListAsync();

        public async Task<Order> GetAsync(int id) =>
            await _db.Orders
                .Include(order => order.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.ProductDimension)
                .ThenInclude(pd => pd.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(order => order.Id == id);



        public async Task<IEnumerable<Order>> GetAsync(string value)
        {
            if (value == null)
            {
                return await _db.Orders
                .Include(order => order.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.ProductDimension)
                .AsNoTracking()
                .ToListAsync();
            }
            else
            {
                string[] parameters = value.Split(' ');
                return await _db.Orders
                .Include(order => order.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.ProductDimension)
                    .Where(order => parameters
                        .Any(parameter =>
                            order.CustomerName.StartsWith(parameter) ||
                            order.InvoiceNumber.ToString().StartsWith(parameter)))
                    .AsNoTracking()
                    .ToListAsync();
            }

        }

        public async Task<Order> UpsertAsync(Order order)
        {
            var existing = await _db.Orders.FirstOrDefaultAsync(_order => _order.Id == order.Id);
            if (existing == null)
            {
                order.InvoiceNumber = (_db.Orders.Max(_order => _order.InvoiceNumber as int?) ?? 0) + 1;

                //Add order
                //We use this line instead of _db.Orders.Add(order)
                //because of a bug which causes navigational items like products and productDimensions
                //to be re-inserted as new records
                _db.ChangeTracker.TrackGraph(order, node => node.Entry.State = !node.Entry.IsKeySet ? EntityState.Added : EntityState.Unchanged);
            }
            else
            {
                // Load the dimensions for the existing product
                _db.Entry(existing).Collection(b => b.OrderDetails).Load();

                // Get the list of the dimensions that were removed from the existing product
                var OrderDetails = existing.OrderDetails.Exclude(order.OrderDetails, i => i.Id).ToList();

                foreach (var doc in OrderDetails)
                {
                    // Remove the relationship between the dimensions and the product
                    existing.OrderDetails.Remove(doc);

                }

                // Get the list of the newly added dimensions
                var addedOrderDetails = order.OrderDetails.Exclude(existing.OrderDetails, i => i.Id).ToList();

                foreach (var od in addedOrderDetails)
                {
                    // The document exists in the repository, so we just attach it to the context
                    _db.OrderDetails.Attach(od);

                    // Create the relation between the batch and document
                    existing.OrderDetails.Add(od);
                }

                // Overwrite all property current values from modified product' entity values, 
                // so that it will have all modified values and mark entity as modified.
                _db.Entry(existing).CurrentValues.SetValues(order);
            }

            await _db.SaveChangesAsync();
            return order;
        }

        public async Task DeleteAsync(int orderId)
        {
            await _db.Orders.Where(p => p.Id == orderId).ExecuteDeleteAsync();
        }
    }
}