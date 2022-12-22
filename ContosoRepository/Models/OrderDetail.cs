using System;

namespace Decorator.DataAccess
{
    public class OrderDetail : DbObject
    {
        public Guid OrderId { get; set; }

        public Order Order { get; set; }

        public Guid ProductDimensionId { get; set; }
        public ProductDimension ProductDimension { get;set; }

        public int Quantity { get; set; } = 1;
        public decimal Price { get; set; }
    }
}