using Decorator.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Decorator.DataAccess
{

    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _db; 

        public CustomerRepository(ApplicationDbContext db)
        {
            _db = db; 
        }

        public async Task<IEnumerable<Customer>> GetAsync()
        {
            return await _db.Customers
                .Include(x=> x.Orders)
                .ToListAsync();
        }

        public async Task<Customer> GetAsync(Guid id)
        {
            return await _db.Customers
                .Include(x => x.Orders)
                .FirstOrDefaultAsync(customer => customer.Id == id);
        }

        public async Task<IEnumerable<Customer>> GetAsync(string value)
        {
            if(value == null)
            {
                return await _db.Customers
                            .Include(x => x.Orders)
                            .ToListAsync();
            }
            else
            {
                string[] parameters = value.Split(' ');
                return await _db.Customers
                    .Where(customer =>
                        parameters.Any(parameter =>
                            customer.FirstName.StartsWith(parameter) ||
                            customer.Phone.StartsWith(parameter)))
                    .Include(x => x.Orders)
                    .ToListAsync();
            }
        }

        public async Task<Customer> UpsertAsync(Customer customer)
        {
            var current = await _db.Customers.FirstOrDefaultAsync(_customer => _customer.Id == customer.Id);
            if (current == null)
            {
                _db.Customers.Add(customer);
            }
            else
            {
                _db.Entry(current).CurrentValues.SetValues(customer);
            }
            await _db.SaveChangesAsync();
            return customer;
        }

        public async Task DeleteAsync(Guid id)
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(_customer => _customer.Id == id);
            if (customer != null)
            {
                var orders = await _db.Orders.Where(order => order.CustomerId == id).ToListAsync();
                _db.Orders.RemoveRange(orders);
                _db.Customers.Remove(customer);
                await _db.SaveChangesAsync();
            }
        }
    }
}
