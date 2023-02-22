using System;
using System.Collections.Generic;
using System.Linq;

namespace Decorator.DataAccess.Models.DatabaseModels
{
    public class CustomOrder
    {
        public int Id { get; set; }
        public int InvoiceNumber { get; set; }

        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string CustomerName { get; set; }

        public List<CustomOrderItem> CustomOrderItems { get; set; } = new List<CustomOrderItem>();

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        public float Discount { get; set; }

        public float SubTotal => CustomOrderItems.Sum(x => x.Price * x.Quantity);

        public float GrandTotal => SubTotal - Discount;

        public float PaidAmount { get; set; }

        public float UnpaidAmount => GrandTotal - PaidAmount;
    }
}
