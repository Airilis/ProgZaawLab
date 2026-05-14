using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderFlow.OrderFlowApp.Models;
using OrderFlow.OrderFlowApp.Services;

namespace OrderFlow.Tests;
public class OrderProcessorTests
{

    [Fact]
    public void FilterOrders_OnlyCompletedOrders_ReturnsFilteredOrders()
    {
        // Arrange
        var orders = new List<Order>
    {
        CreateOrder(1, OrderStatus.Completed, 100),
        CreateOrder(2, OrderStatus.New, 200),
        CreateOrder(3, OrderStatus.Completed, 300)
    };

        var processor = new OrderProcessor(orders);

        // Act
        var result = processor.FilterOrders(
            o => o.Status == OrderStatus.Completed);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.All(result,
            o => Assert.Equal(OrderStatus.Completed, o.Status));
    }

    [Fact]
    public void AggregateOrders_SumTotalAmount_ReturnsCorrectSum()
    {
        // Arrange
        var orders = new List<Order>
    {
        CreateOrder(1, OrderStatus.New, 100),
        CreateOrder(2, OrderStatus.New, 200),
        CreateOrder(3, OrderStatus.New, 300)
    };

        var processor = new OrderProcessor(orders);

        // Act
        var result = processor.AggregateOrders(
            o => o.Sum(x => x.TotalAmount));

        // Assert
        Assert.Equal(600, result);
    }

    // -----------------------------
    // Helpers
    // -----------------------------

    private Order CreateOrder(
        int id,
        OrderStatus status,
        decimal amount)
    {
        return new Order
        {
            Id = id,

            Status = status,

            Customer = new Customer
            {
                FullName = "Test User"
            },

            Items = new List<OrderItem>
        {
            new OrderItem
            {
                Quantity = 1,
                UnitPrice = amount,

                Product = new Product
                {
                    Name = "Product",
                    Price = amount
                }
            }
        }
        };
    }
}
