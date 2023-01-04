
using System;

namespace Decorator.DataAccess;

public class ProductDimension : Entity
{
    public Product Product { get; set; }
    public int ProductId { get; set; }
    public float DimensionX { get; set; }
    public float DimensionY { get; set; }
    public int Quantity { get; set; }
    public float Price { get; set; }

    public string ProductFullName => $"{Product.Name} - {DimensionX} × {DimensionY} سم";

}