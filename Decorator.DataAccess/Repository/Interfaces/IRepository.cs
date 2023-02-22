namespace Decorator.DataAccess;

public interface IRepository
{
    IOrderRepository Orders { get; }
    ICustomOrderRepository CustomOrders { get; }

    IProductRepository Products { get; }
}
