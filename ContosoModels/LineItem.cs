using Newtonsoft.Json;
using System;

namespace Contoso.Models
{
    /// <summary>
    /// Represents a line item (product + quantity) on an order.
    /// </summary>
    public class LineItem : DbObject
    {
        /// <summary>
        /// Gets or sets the id of the order the line item is associated with.
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// Gets or sets the order the line item is associated with.
        /// </summary>
        [JsonIgnore]
        public Order Order { get; set; }

        /// <summary>
        /// Gets or sets the product's id.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product.
        /// </summary>
        public Product Product { get; set; }

        /// <summary>
        /// Gets or sets the quantity of product. 
        /// </summary>
        public int Quantity { get; set; } = 1; 
    }
}