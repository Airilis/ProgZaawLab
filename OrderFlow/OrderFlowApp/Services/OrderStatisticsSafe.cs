using OrderFlow.OrderFlowApp.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Services
{
    public class OrderStatisticsSafe
    {
        public int TotalProcessed = 0;
        public decimal TotalRevenue = 0;

        public ConcurrentDictionary<OrderStatus, int> OrdersPerStatus = new();
        public List<string> ProcessingErrors = new();

        private readonly object _lock = new();

        public void AddOrder(Order order)
        {
            //  thread-safe licznik
            Interlocked.Increment(ref TotalProcessed);

            //  lock dla revenue i listy
            lock (_lock)
            {
                TotalRevenue += order.TotalAmount;

                if (order.Status != OrderStatus.Completed)
                    ProcessingErrors.Add($"Order {order.Id} not completed");
            }

            //  ConcurrentDictionary
            OrdersPerStatus.AddOrUpdate(order.Status,1,(key, oldValue) => oldValue + 1);
        }
    }
}
