using System;

namespace Decorator.DataAccess.Models
{
    public class DbObject
    {
        public Guid Id { get; set; } = Guid.NewGuid(); 
    }
}
