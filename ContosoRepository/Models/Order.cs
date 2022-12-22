using System;
using System.Collections.Generic;
using System.Linq;

namespace Decorator.DataAccess.Models
{
    /// <summary>
    /// Represents a customer order.
    /// </summary>
    public class Order : DbObject
    {
        /// <summary>
        /// Creates a new order.
        /// </summary>
        public Order()
        { }

        /// <summary>
        /// Creates a new order for the given customer.
        /// </summary>
        public Order(Customer customer) : this()
        {
            Customer = customer;
            CustomerName = $"{customer.FirstName} {customer.LastName}";
            CustomerId = customer.Id;
            Address = customer.Address;
        }

        public Guid CustomerId { get; set; }

        public Customer Customer { get; set; }

        public string CustomerName { get; set; }

        public int InvoiceNumber { get; set; } = 0;

        public string Address { get; set; }

        public List<LineItem> LineItems { get; set; } = new List<LineItem>();

        public DateTime DatePlaced { get; set; } = DateTime.Now;

        public DateTime? DateFilled { get; set; } = null;

        public OrderStatus Status { get; set; } = OrderStatus.Open;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        public Term Term { get; set; }

        public decimal Subtotal => LineItems.Sum(lineItem => lineItem.Product.ListPrice * lineItem.Quantity);

        public decimal Tax => Subtotal * .095m;

        public decimal GrandTotal => Subtotal + Tax; 

        public override string ToString() => InvoiceNumber.ToString();
    }

    public enum Term
    {
        Net1, 
        Net5,
        Net15, 
        Net30
    }

    public enum PaymentStatus
    {
        Unpaid,
        Paid 
    }

    public enum OrderStatus
    {
        Open,
        Filled, 
        Cancelled
    }
}
