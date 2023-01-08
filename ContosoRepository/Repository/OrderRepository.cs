using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
                return await _db.Orders
                .Include(order => order.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.ProductDimension)
                .Where(o => o.CustomerName.StartsWith(value) || o.InvoiceNumber.ToString().StartsWith(value))                   
                .AsNoTracking()
                .ToListAsync();
            }

        }

        public async Task<Order> UpsertAsync(Order order)
        {
            var existing = await _db.Orders
                .Include(x => x.OrderDetails)
                .ThenInclude(x => x.ProductDimension)
                .FirstOrDefaultAsync(_order => _order.Id == order.Id);

            if (existing == null)
            {
                order.InvoiceNumber = (_db.Orders.Max(_order => _order.InvoiceNumber as int?) ?? 0) + 1;

                //Add order
                //We use this line instead of _db.Orders.Add(order)
                //because of a bug which causes navigational items like products and productDimensions
                //to be re-inserted as new records
                _db.ChangeTracker.TrackGraph(order, node => node.Entry.State = !node.Entry.IsKeySet ? EntityState.Added : EntityState.Unchanged);

                foreach (var od in order.OrderDetails)
                {
                    od.ProductDimension.Quantity -= od.Quantity;
                }

                await _db.SaveChangesAsync();
            }
            else
            {
                // Get the list of the order details that were removed from the existing order
                var removedOrderDetails = existing.OrderDetails.Exclude(order.OrderDetails, i => i.Id).ToList();

                // Get the list of the newly added order details
                var addedOrderDetails = order.OrderDetails.Exclude(existing.OrderDetails, i => i.Id).ToList();

                var existingOrderDetails = existing.OrderDetails.Except(removedOrderDetails);

                foreach (var od in removedOrderDetails)
                {
                    //When an order detail is removed this means that a product was returned
                    //So we modify its stock to reflect that
                    od.ProductDimension.Quantity += od.Quantity;

                    // Remove the relationship between the order details and the order
                    existing.OrderDetails.Remove(od);
                }

                foreach (var od in addedOrderDetails)
                {
                    od.ProductDimension.Quantity -= od.Quantity;

                    // Create the relation between the order and orderDetail
                    existing.OrderDetails.Add(od);
                }

                foreach (OrderDetail od in existingOrderDetails)
                {
                    int modfiedQuantity = order.OrderDetails.FirstOrDefault(x => x.Id == od.Id).Quantity;
                    int orginalQuantity = od.Quantity;
                    int difference = orginalQuantity - modfiedQuantity;

                    //Check if there is a change in quantity
                    //and increase/decrease the stock in ProductDimension
                    if(difference > 0)
                        od.ProductDimension.Quantity += difference;
                    else if(difference < 0)
                        od.ProductDimension.Quantity -= Math.Abs(difference);

                    //Also update orderDetail as updating the existing order doesn't update its order details
                    od.Quantity = modfiedQuantity;
                    //TODO: Ask whether to update price
                }

                // Overwrite all property current values from modified order' entity values, 
                // so that it will have all modified values and mark entity as modified.
                //Doesn't include navigational properties
                _db.Entry(existing).CurrentValues.SetValues(order);            

                await _db.SaveChangesAsync();
            }

            return order;
        }

        public async Task DeleteAsync(int orderId)
        {
            await _db.Orders.Where(p => p.Id == orderId).ExecuteDeleteAsync();
        }
    }
}