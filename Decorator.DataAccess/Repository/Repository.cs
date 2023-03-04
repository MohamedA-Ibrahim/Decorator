using Microsoft.EntityFrameworkCore;

namespace Decorator.DataAccess
{
    public class Repository : IRepository
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions; 

        public Repository(DbContextOptionsBuilder<ApplicationDbContext> 
            dbOptionsBuilder)
        {
            _dbOptions = dbOptionsBuilder.Options;
            using (var db = new ApplicationDbContext(_dbOptions))
            {
                db.Database.EnsureCreated(); 
            }
        }

        public IOrderRepository Orders => new OrderRepository(new ApplicationDbContext(_dbOptions));

        public IProductRepository Products => new ProductRepository(new ApplicationDbContext(_dbOptions));

        public ICustomOrderRepository CustomOrders => new CustomOrderRepository(new ApplicationDbContext(_dbOptions));

    }
}
