using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderFlow.OrderFlowApp.Models;

namespace OrderFlow.OrderFlowApp.Persistence
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(OrderFlowContext db)
        {
            if (await db.Products.AnyAsync())
                return;

            var products = new List<Product>
            {
                new Product { Name = "Laptop", Category = "Electronics", Price = 5000 },
                new Product { Name = "Mouse", Category = "Electronics", Price = 100 },
                new Product { Name = "Keyboard", Category = "Electronics", Price = 200 },
                new Product { Name = "Monitor", Category = "Electronics", Price = 1200 },
                new Product { Name = "Desk", Category = "Furniture", Price = 800 }
            };

            var customers = new List<Customer>
            {
                new Customer { FullName = "Jan Kowalski", IsVip = false },
                new Customer { FullName = "Anna Nowak", IsVip = true },
                new Customer { FullName = "Piotr Wiśniewski", IsVip = false },
                new Customer { FullName = "Katarzyna Zielińska", IsVip = true }
            };

            await db.AddRangeAsync(products);
            await db.AddRangeAsync(customers);
            await db.SaveChangesAsync();
            var orders = new List<Order>
            {
                new Order
                {
                    CustomerId = customers[0].Id,
                    Status = OrderStatus.New,
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = products[0].Id,
                            Quantity = 1,
                            UnitPrice = products[0].Price
                        },
                        new OrderItem
                        {
                            ProductId = products[1].Id,
                            Quantity = 2,
                            UnitPrice = products[1].Price
                        }
                    }
                },

                new Order
                {
                    CustomerId = customers[1].Id,
                    Status = OrderStatus.Completed,
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = products[2].Id,
                            Quantity = 1,
                            UnitPrice = products[2].Price
                        }
                    }
                },

                new Order
                {
                    CustomerId = customers[2].Id,
                    Status = OrderStatus.Cancelled,
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = products[3].Id,
                            Quantity = 1,
                            UnitPrice = products[3].Price
                        }
                    }
                }
            };

            await db.Orders.AddRangeAsync(orders);
            await db.SaveChangesAsync();

            Console.WriteLine("Seed danych wykonany");

        }
        
    }
}

