using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderFlow.OrderFlowApp.Models;

namespace OrderFlow.OrderFlowApp.Services
{
    public class OrderProcessor
    {
        private List<Order> orders;
        public OrderProcessor(List<Order> orders)
        {
            this.orders = orders;
        }
        // ---------- Predicate<Order> ----------
        public List<Order> FilterOrders(Predicate<Order> predicate)
        {
            return orders.FindAll(predicate);
        }
        // ---------- Action<Order> ----------
        public void ApplyAction(Action<Order> action, List<Order> targetOrders)
        {
            foreach (var order in targetOrders)
                action(order);
        }
        // ---------- Func<Order, T> ----------
        public List<T> ProjectOrders<T>(Func<Order, T> selector)
        {
            return orders.Select(selector).ToList();
        }
        // ---------- Agregacja ----------
        public decimal AggregateOrders(Func<IEnumerable<Order>, decimal> aggregator)
        {
            return aggregator(orders);
        }
        // ---------- Przykładowy flow ----------
        public void FilterSortTakePrint()
        {
            // Predykaty
            Predicate<Order> isVip = o => o.Customer.IsVip;
            Predicate<Order> bigOrder = o => o.TotalAmount > 1000;
            Predicate<Order> completed = o => o.Status == OrderStatus.Completed;

            var filtered = orders
                .Where(o => isVip(o) && bigOrder(o) && !completed(o))
                .OrderByDescending(o => o.TotalAmount)
                .Take(3)
                .ToList();
            // Action - wypisz i zmień status na Processing
            Action<Order> print = o => Console.WriteLine($"Order {o.Id} for {o.Customer.FullName}: {o.TotalAmount} zł");
            Action<Order> markProcessing = o => o.Status = OrderStatus.Processing;

            foreach (var order in filtered)
            {
                print(order);
                markProcessing(order);
            }
        }
    }
}
