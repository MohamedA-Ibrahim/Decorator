using System;

namespace Decorator.DataAccess
{
    public class DbObject
    {
        public Guid Id { get; set; } = Guid.NewGuid(); 
    }
}
