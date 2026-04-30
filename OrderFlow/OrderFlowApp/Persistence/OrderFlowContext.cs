using Microsoft.EntityFrameworkCore;
using OrderFlow.OrderFlowApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Persistence
{
    public class OrderFlowContext : DbContext
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options
                .UseSqlite("Data Source=orderflow.db")
                .LogTo(Console.WriteLine);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Customer -> Order (1:N)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order -> OrderItem (1:N)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem -> Product (N:1)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId);

            // Ignore computed
            modelBuilder.Entity<Order>().Ignore(o => o.TotalAmount);
            modelBuilder.Entity<OrderItem>().Ignore(oi => oi.TotalPrice);

            // Precision
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(p => p.UnitPrice)
                .HasPrecision(18, 2);

            // Indexes
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.FullName);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Status);
        }
    }
}
