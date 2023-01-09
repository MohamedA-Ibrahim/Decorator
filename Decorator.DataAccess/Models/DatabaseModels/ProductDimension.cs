
using System;
using System.Collections.Generic;

namespace Decorator.DataAccess;

public class ProductDimension : Entity
{
    public Product Product { get; set; }
    public int ProductId { get; set; }
    public float DimensionX { get; set; }
    public float DimensionY { get; set; }
    public int Quantity { get; set; }
    public float Price { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; }

    public string ProductFullName => (DimensionX == 0 || DimensionY == 0) 
        ? Product.Name
        : $"{Product.Name} - {DimensionX} × {DimensionY} سم";
}