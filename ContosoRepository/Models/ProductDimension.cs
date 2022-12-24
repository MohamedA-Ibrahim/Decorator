
using System;

namespace Decorator.DataAccess;

public class ProductDimension : DbObject
{
    public Product Product { get; set; }
    public int ProductId { get; set; }
    public float DimensionX { get; set; }
    public float DimensionY { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}