using System;

namespace Decorator.DataAccess
{
    public class OrderDetail : Entity
    {
        public int OrderId { get; set; }

        public Order Order { get; set; }

        public int ProductDimensionId { get; set; }

        public ProductDimension ProductDimension { get;set; }

        public int Quantity { get; set; } = 1;
        public float Price { get; set; }
    }
}