using System;
using System.Collections.Generic;
using System.Linq;

namespace Decorator.DataAccess
{
    public class Order : Entity
    {

        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerName { get; set; }

        public int InvoiceNumber { get; set; } = 0;

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        public float Discount { get; set; }

        public float Subtotal => OrderDetails.Sum(orderDetail => orderDetail.ProductDimension.Price * orderDetail.Quantity);

        public float GrandTotal => Subtotal - Discount;
    }
}
