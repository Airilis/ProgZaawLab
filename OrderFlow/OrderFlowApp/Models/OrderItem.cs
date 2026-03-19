using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Models
{
    public class OrderItem
    {
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal TotalPrice => Product.Price * Quantity;
    }
}
