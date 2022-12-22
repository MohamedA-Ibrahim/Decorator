namespace Decorator.DataAccess;

public interface IRepository
{
    ICustomerRepository Customers { get; }

    IOrderRepository Orders { get; }

    IProductRepository Products { get; }
}
