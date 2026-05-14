using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderFlow.OrderFlowApp.Models;
using OrderFlow.OrderFlowApp.Services;

namespace OrderFlow.Tests;

public class OrderValidatorTests
{
    // -----------------------------
    // HasItems
    // -----------------------------

    [Fact]
    public void HasItems_OrderWithoutItems_ReturnsFalse()
    {
        // Arrange
        var validator = new OrderValidator();

        var order = CreateEmptyOrder();

        // Act
        var result = validator.HasItems(order, out var error);

        // Assert
        Assert.False(result);
        Assert.Equal("Order has no items.", error);
    }

    // -----------------------------
    // TotalAmountUnderLimit
    // -----------------------------

    [Fact]
    public void TotalAmountUnderLimit_AmountAboveLimit_ReturnsFalse()
    {
        // Arrange
        var validator = new OrderValidator();

        var order = CreateValidOrder();

        order.Items.Add(CreateItem(6000, 1));

        // Act
        var result = validator.TotalAmountUnderLimit(order, out var error);

        // Assert
        Assert.False(result);
        Assert.Equal("Total amount exceeds 5000.", error);
    }

    // -----------------------------
    // QuantitiesPositive
    // -----------------------------

    [Fact]
    public void QuantitiesPositive_ItemWithZeroQuantity_ReturnsFalse()
    {
        // Arrange
        var validator = new OrderValidator();

        var order = CreateValidOrder();

        order.Items.Add(CreateItem(100, 0));

        // Act
        var result = validator.QuantitiesPositive(order, out var error);

        // Assert
        Assert.False(result);
        Assert.Contains("non-positive quantity", error);
    }

    // -----------------------------
    // Lambda rule - status
    // -----------------------------

    [Theory]
    [InlineData(OrderStatus.New, true)]
    [InlineData(OrderStatus.Completed, true)]
    [InlineData(OrderStatus.Cancelled, false)]
    public void ValidateAll_StatusValidation_ReturnsExpectedResult(
        OrderStatus status,
        bool expectedValid)
    {
        // Arrange
        var validator = new OrderValidator();

        var order = CreateValidOrder();
        order.Status = status;

        // Act
        var errors = validator.ValidateAll(order);

        // Assert
        if (expectedValid)
        {
            Assert.DoesNotContain(errors,
                e => e.Contains("cancelled"));
        }
        else
        {
            Assert.Contains(errors,
                e => e.Contains("cancelled"));
        }
    }

    // -----------------------------
    // Lambda rule - amount over 10000
    // -----------------------------

    [Fact]
    public void ValidateAll_TotalAmountOver10000_ReturnsError()
    {
        // Arrange
        var validator = new OrderValidator();

        var order = CreateValidOrder();

        order.Items.Add(CreateItem(11000, 1));

        // Act
        var errors = validator.ValidateAll(order);

        // Assert
        Assert.Contains(errors,
            e => e.Contains("10 000"));
    }

    // -----------------------------
    // ValidateAll - multiple errors
    // -----------------------------

    [Fact]
    public void ValidateAll_OrderBreakingMultipleRules_ReturnsMultipleErrors()
    {
        // Arrange
        var validator = new OrderValidator();

        var order = CreateValidOrder();

        order.Status = OrderStatus.Cancelled;

        order.Items.Add(CreateItem(12000, 1));

        order.Items.Add(CreateItem(100, 0));

        // Act
        var errors = validator.ValidateAll(order);

        // Assert
        Assert.Equal(4, errors.Count);
    }

    // -----------------------------
    // Helpers
    // -----------------------------

    private Order CreateEmptyOrder()
    {
        return new Order
        {
            Customer = new Customer()
        };
    }

    private Order CreateValidOrder()
    {
        return new Order
        {
            Customer = new Customer(),

            Status = OrderStatus.New,

            Items = new List<OrderItem>
            {
                CreateItem(100, 1)
            }
        };
    }

    private OrderItem CreateItem(decimal price, int quantity)
    {
        return new OrderItem
        {
            Quantity = quantity,
            UnitPrice = price,
            Product = new Product
            {
                Name = "Test Product",
                Price = price
            },
            
        };
    }
}