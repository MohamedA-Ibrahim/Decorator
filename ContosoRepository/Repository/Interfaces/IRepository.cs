namespace Decorator.DataAccess;

public interface IRepository
{
    IOrderRepository Orders { get; }

    IProductRepository Products { get; }
}
