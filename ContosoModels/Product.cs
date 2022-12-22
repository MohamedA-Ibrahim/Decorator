
namespace Contoso.Models
{
    /// <summary>
    /// Represents a product.
    /// </summary>
    public class Product : DbObject
    { 
        /// <summary>
        /// Gets or sets the product's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the product's color.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the days required to manufacture the product.
        /// </summary>
        public int DaysToManufacture { get; set; }

        /// <summary>
        /// Gets or sets the product's standard cost.
        /// </summary>
        public decimal StandardCost { get; set; }

        /// <summary>
        /// Gets or sets the product's list price.
        /// </summary>
        public decimal ListPrice { get; set; }

        /// <summary>
        /// Gets or sets the product's weight.
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Gets or sets the product's description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Returns the name of the product and the list price.
        /// </summary>
        public override string ToString() => $"{Name} \n{ListPrice}";
    }
}