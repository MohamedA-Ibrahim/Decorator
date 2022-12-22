using System.Collections.Generic;

namespace Decorator.DataAccess;

public class Product : DbObject
{ 
    public string Name { get; set; }

    public string Code { get; set; }

    public ICollection<ProductDimension> ProductDimensions { get; set; }
}
