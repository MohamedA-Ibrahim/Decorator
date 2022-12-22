using System;
using System.Collections.Generic;

namespace Decorator.DataAccess.Models
{
    public class Customer : DbObject, IEquatable<Customer>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string Company { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }
        public List<Order> Orders { get; set; }

        public override string ToString() => $"{FirstName} {LastName}";

        public bool Equals(Customer other) =>
            FirstName == other.FirstName &&
            LastName == other.LastName &&
            Company == other.Company &&
            Email == other.Email &&
            Phone == other.Phone &&
            Address == other.Address;
    }
}
