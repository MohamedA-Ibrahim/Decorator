using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decorator.DataAccess
{
    public class ProductOrdersDTO
    {
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerName { get; set; }
        public int InvoiceNumber { get; set; }

        public int Quantity { get; set; }
        public float Price { get; set; }

        public DateTime PurchaseDate { get; set; }
        public string ProductName { get; set; }

        //Used temporarly until QuestPDF fix arabic letters with numbers bug
        public float DimensionX { get; set; }
        public float DimensionY { get; set; }
    }
}
