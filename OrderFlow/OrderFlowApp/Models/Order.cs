using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public Customer Customer { get; set; } = null!;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    }
}
