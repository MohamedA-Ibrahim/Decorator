using Decorator.DataAccess.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Decorator.DataAccess
{
    public class CustomOrderRepository : ICustomOrderRepository
    {
        private readonly ApplicationDbContext _db;

        public CustomOrderRepository(ApplicationDbContext db) => _db = db;

        public async Task<IEnumerable<CustomOrder>> GetAsync() =>
            await _db.CustomOrders
                .Include(order => order.CustomOrderItems)
                .AsNoTracking()
                .ToListAsync();

        public async Task<CustomOrder> GetAsync(int id) =>
            await _db.CustomOrders
                .Include(order => order.CustomOrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(order => order.Id == id);

        public async Task<IEnumerable<CustomOrder>> GetAsync(string value)
        {
            if (value == null)
            {
                return await _db.CustomOrders
                .Include(order => order.CustomOrderItems)
                .AsNoTracking()
                .ToListAsync();
            }

            return await _db.CustomOrders
            .Include(order => order.CustomOrderItems)
            .Where(o => o.CustomerName.StartsWith(value))
            .AsNoTracking()
            .ToListAsync();
        }

        public async Task<CustomOrder> UpsertAsync(CustomOrder order)
        {
            var existing = await _db.CustomOrders
                .Include(x => x.CustomOrderItems)
                .FirstOrDefaultAsync(_order => _order.Id == order.Id);

            if (existing == null)
            {
                order.InvoiceNumber = (_db.CustomOrders.Max(_order => _order.InvoiceNumber as int?) ?? 0) + 1;

                await _db.CustomOrders.AddAsync(order);
                await _db.SaveChangesAsync();
            }
            else
            {
                existing.CustomerName = order.CustomerName;
                existing.CustomerAddress = order.CustomerAddress;
                existing.CustomerPhone = order.CustomerPhone;
                existing.Discount = order.Discount;

                //To fix the error: "The instance of entity type 'CustomOrderItem' cannot be tracked
                //because another instance with the same key value for {'Id'} is already being tracked."
                foreach (var item in existing.CustomOrderItems)
                {
                    _db.Entry(item).State = EntityState.Detached;
                }

                existing.CustomOrderItems = order.CustomOrderItems;

                _db.CustomOrders.Update(existing);
                await _db.SaveChangesAsync();
            }

            return order;
        }

        public async Task DeleteAsync(int orderId)
        {
            var order = await _db.CustomOrders
                                 .Include(x => x.CustomOrderItems)
                                 .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
                return;

            _db.CustomOrders.Remove(order);

            await _db.SaveChangesAsync();
        }
    }
}