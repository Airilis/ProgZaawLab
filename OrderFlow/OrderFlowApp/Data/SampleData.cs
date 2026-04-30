using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Data
{
    using Models;
    public static class SampleData
    {        
        public static List<Product> Products { get; } = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Category = "Electronics", Price = 3500m },
            new Product { Id = 2, Name = "Smartphone", Category = "Electronics", Price = 2000m },
            new Product { Id = 3, Name = "Chair", Category = "Furniture", Price = 300m },
            new Product { Id = 4, Name = "Desk", Category = "Furniture", Price = 800m },
            new Product { Id = 5, Name = "Book", Category = "Books", Price = 50m },
        };
        public static List<Customer> Customers { get; } = new List<Customer>
        {
            new Customer { Id = 1, FullName = "Alice", IsVip = true },
            new Customer { Id = 2, FullName = "Bob" },
            new Customer { Id = 3, FullName = "Charlie" },
            new Customer { Id = 4, FullName = "Diana" },
        };
        public static List<Order> Orders { get; } = new List<Order>
        {
            new Order
            {
                Id = 1,
                Customer = Customers[0],
                Status = OrderStatus.New,
                Items = new List<OrderItem>
                {
                    new OrderItem { Product = Products[0], Quantity = 1 },
                    new OrderItem { Product = Products[4], Quantity = 5 },
                }
            },
            new Order
            {
                Id = 2,
                Customer = Customers[1],
                Status = OrderStatus.Validated,
                Items = new List<OrderItem>
                {
                    new OrderItem { Product = Products[1], Quantity = 2 }
                }
            },
            new Order
            {
                Id = 3,
                Customer = Customers[2],
                Status = OrderStatus.Processing,
                Items = new List<OrderItem>
                {
                    new OrderItem { Product = Products[2], Quantity = 4 },
                    new OrderItem { Product = Products[3], Quantity = 1 }
                }
            },
            new Order
            {
                Id = 4,
                Customer = Customers[3],
                Status = OrderStatus.Completed,
                Items = new List<OrderItem>
                {
                    new OrderItem { Product = Products[4], Quantity = 10 }
                }
            },
            new Order
            {
                Id = 5,
                Customer = Customers[0],
                Status = OrderStatus.Cancelled,
                Items = new List<OrderItem>
                {
                    new OrderItem { Product = Products[1], Quantity = 1 },
                    new OrderItem { Product = Products[3], Quantity = 2 }
                }
            },
            new Order
            {
                Id = 6,
                Customer = Customers[2],
                Status = OrderStatus.New,
                Items = new List<OrderItem>
                {
                    new OrderItem { Product = Products[0], Quantity = 1 },
                    new OrderItem { Product = Products[2], Quantity = 2 }
                }
            },
        };
    }
}
