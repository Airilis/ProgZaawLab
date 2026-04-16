using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderFlow.OrderFlowApp.Models;

namespace OrderFlow.OrderFlowApp.Services
{
    public class OrderStatistics
    {
        //błędna wersja zkomentowana 
        public int TotalProcessed = 0;
        public decimal TotalRevenue = 0;
        public Dictionary<OrderStatus, int> OrdersPerStatus = new();
        public List<string> ProcessingErrors = new();

        public void AddOrder(Order order)
        {
            TotalProcessed++; //  nie thread-safe

            TotalRevenue += order.TotalAmount; //  nie thread-safe

            if (!OrdersPerStatus.ContainsKey(order.Status))
                OrdersPerStatus[order.Status] = 0;

            OrdersPerStatus[order.Status]++; //  nie thread-safe

            if (order.Status != OrderStatus.Completed)
                ProcessingErrors.Add($"Order {order.Id} not completed"); //  nie thread-safe
        }
    }
}
