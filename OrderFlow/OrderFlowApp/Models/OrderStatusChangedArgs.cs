using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Models
{
    public class OrderStatusChangedEventArgs : EventArgs
    {
        public Order Order { get; set; }
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public DateTime Timestamp { get; set; }

        public OrderStatusChangedEventArgs(Order order, OrderStatus oldStatus, OrderStatus newStatus)
        {
            Order = order;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            Timestamp = DateTime.Now;
        }
    }
}
