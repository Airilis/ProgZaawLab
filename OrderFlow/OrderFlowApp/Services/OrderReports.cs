using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderFlow.OrderFlowApp.Models;
using OrderFlow.OrderFlowApp.Data;

namespace OrderFlow.OrderFlowApp.Services
{
    public static class OrderReports
    {
        // Metoda pokazująca wszystkie raporty LINQ
        public static void RunReports(List<Order> orders)
        {
            // ------------------------
            // 1️. Join zamówień z klientami
            // Method syntax: grupowanie zamówień po nazwie klienta (może być miasto w realnym przykładzie)
            // ------------------------
            var ordersByCustomer = orders
                .Join(SampleData.Customers,
                      o => o.Customer.Id,
                      c => c.Id,
                      (o, c) => new { CustomerName = c.FullName, OrderId = o.Id, Total = o.TotalAmount })
                .GroupBy(x => x.CustomerName)
                .ToList();

            Console.WriteLine("=== Orders grouped by customer ===");
            foreach (var grp in ordersByCustomer)
            {
                Console.WriteLine($"Customer: {grp.Key}");
                foreach (var o in grp)
                    Console.WriteLine($"  Order {o.OrderId}: {o.Total} zł");
            }

            // ------------------------
            // 2️. SelectMany - spłaszczenie Order → OrderItems → Product
            // Query syntax
            // ------------------------
            var allProducts =
                from o in orders
                from item in o.Items
                select item.Product;

            Console.WriteLine("\n=== All Products in all orders ===");
            foreach (var p in allProducts.Distinct())
                Console.WriteLine($"Product: {p.Name}, Category: {p.Category}");

            // ------------------------
            // 3️. GroupBy z agregacją - top klienci wg kwoty
            // Method syntax
            // ------------------------
            var topCustomers = orders
                .GroupBy(o => o.Customer.FullName)
                .Select(g => new { Customer = g.Key, TotalSpent = g.Sum(o => o.TotalAmount) })
                .OrderByDescending(x => x.TotalSpent)
                .Take(3);

            Console.WriteLine("\n=== Top 3 customers by total amount ===");
            foreach (var c in topCustomers)
                Console.WriteLine($"{c.Customer}: {c.TotalSpent} zł");

            // ------------------------
            // 4️. Średnia wartość zamówień per kategoria
            // Mixed syntax: query + method
            // ------------------------
            var avgPerCategory =
                (from o in orders
                 from item in o.Items
                 group item by item.Product.Category into g
                 select new
                 {
                     Category = g.Key,
                     AvgPrice = g.Average(x => x.TotalPrice)
                 })
                .OrderByDescending(x => x.AvgPrice);

            Console.WriteLine("\n=== Average order value per category ===");
            foreach (var c in avgPerCategory)
                Console.WriteLine($"{c.Category}: {c.AvgPrice:F2} zł");

            // ------------------------
            // 5️. GroupJoin (left join pattern) - lista wszystkich klientów z ich zamówieniami
            // Method syntax
            // ------------------------
            var customerOrders = SampleData.Customers
                .GroupJoin(orders,
                           c => c.Id,
                           o => o.Customer.Id,
                           (c, ords) => new { Customer = c.FullName, Orders = ords })
                .ToList();

            Console.WriteLine("\n=== Customers with their orders (left join) ===");
            foreach (var c in customerOrders)
            {
                Console.WriteLine($"Customer: {c.Customer}");
                if (c.Orders.Any())
                    foreach (var o in c.Orders)
                        Console.WriteLine($"  Order {o.Id}: {o.TotalAmount} zł");
                else
                    Console.WriteLine("  No orders");
            }

            // ------------------------
            // 6️. Raport per klient z ulubioną kategorią
            // Mixed syntax: query syntax + method syntax
            // ------------------------
            var favCategoryPerCustomer =
                from o in orders
                from item in o.Items
                group item by o.Customer.FullName into g
                select new
                {
                    Customer = g.Key,
                    FavoriteCategory = g.GroupBy(i => i.Product.Category)
                                        .OrderByDescending(gr => gr.Sum(x => x.TotalPrice))
                                        .First().Key
                };

            Console.WriteLine("\n=== Favorite category per customer ===");
            foreach (var c in favCategoryPerCustomer)
                Console.WriteLine($"{c.Customer}: {c.FavoriteCategory}");
        }
    }
}
