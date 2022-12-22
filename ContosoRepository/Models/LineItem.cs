using System;

namespace Decorator.DataAccess.Models
{
    /// <summary>
    /// Represents a line item (product + quantity) on an order.
    /// </summary>
    public class LineItem : DbObject
    {
        public Guid OrderId { get; set; }

        public Order Order { get; set; }

        public Guid ProductId { get; set; }

        public Product Product { get; set; }

        public int Quantity { get; set; } = 1; 
    }
}