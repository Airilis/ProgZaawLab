using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderFlow.OrderFlowApp.Services;
using OrderFlow.OrderFlowApp.Models;

namespace OrderFlow.Tests
{
    public class DiscountCalculatorTests
    {
        [Fact]
        public void StandardCustomer_SmallOrder_ReturnsZeroDiscount()
        {
            // Arrange
            var calculator = new DiscountCalculator();

            var order = new Order
            {
                Customer = new Customer
                {
                    IsVip = false
                },
                Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Quantity = 1,
                    UnitPrice = 100,
                    Product = new Product { Price = 100 }
                }
            }
            };

            // Act
            var discount = calculator.CalculateDiscount(order);

            // Assert
            Assert.Equal(0, discount);
        }
        [Fact]
        public void VipCustomer_SmallOrder_Returns10PercentDiscount()
        {
            // Arrange
            var calculator = new DiscountCalculator();

            var order = new Order
            {
                Customer = new Customer
                {
                    IsVip = true
                },
                Items = new List<OrderItem>
        {
            new OrderItem
            {
                Quantity = 1,
                UnitPrice = 100,
                Product = new Product { Price = 100 }
            }
        }
            };

            // Act
            var discount = calculator.CalculateDiscount(order);

            // Assert
            Assert.Equal(10, discount);
        }
        [Fact]
        public void OrderOver1000_Adds5PercentDiscount()
        {
            // Arrange
            var calculator = new DiscountCalculator();

            var order = new Order
            {
                Customer = new Customer
                {
                    IsVip = false
                },
                Items = new List<OrderItem>
        {
            new OrderItem
            {
                Quantity = 1,
                UnitPrice = 2000,
                Product = new Product { Price = 2000 }
            }
        }
            };

            // Act
            var discount = calculator.CalculateDiscount(order);

            // Assert
            Assert.Equal(100, discount); // 5% z 2000
        }
        [Fact]
        public void VipCustomer_OrderOver5000_Returns20PercentDiscount()
        {
            // Arrange
            var calculator = new DiscountCalculator();

            var order = new Order
            {
                Customer = new Customer
                {
                    IsVip = true
                },
                Items = new List<OrderItem>
        {
            new OrderItem
            {
                Quantity = 1,
                UnitPrice = 6000,
                Product = new Product { Price = 6000 }
            }
        }
            };

            // Act
            var discount = calculator.CalculateDiscount(order);

            // Assert
            Assert.Equal(1200, discount); // 20% z 6000
        }
        [Fact]
        public void Discount_NeverExceeds25Percent()
        {
            // Arrange
            var calculator = new DiscountCalculator();

            var order = new Order
            {
                Customer = new Customer
                {
                    IsVip = true
                },
                Items = new List<OrderItem>
        {
            new OrderItem
            {
                Quantity = 1,
                UnitPrice = 20000,
                Product = new Product { Price = 20000 }
            }
        }
            };

            // Act
            var discount = calculator.CalculateDiscount(order);

            // Assert
            Assert.Equal(4000, discount); // max 25% z 20000
        }
        [Fact]
        public void StandardCustomer_SmallOrder_NoDiscount_ReturnsZero()
        {
            var calculator = new DiscountCalculator();

            var order = new Order
            {
                Customer = new Customer { IsVip = false },
                Items = new List<OrderItem>
        {
            new OrderItem { Quantity = 1, UnitPrice = 100, Product = new Product { Price = 100 } }
        }
            };

            var discount = calculator.CalculateDiscount(order);

            Assert.Equal(0, discount);
        }
        [Fact]
        public void StandardCustomer_OrderOver1000_Returns5Percent()
        {
            var calculator = new DiscountCalculator();

            var order = new Order
            {
                Customer = new Customer { IsVip = false },
                Items = new List<OrderItem>
        {
            new OrderItem { Quantity = 1, UnitPrice = 2000, Product = new Product { Price = 2000 } }
        }
            };

            var discount = calculator.CalculateDiscount(order);

            Assert.Equal(100, discount); // 5%
        }
        [Fact]
        public void VipCustomer_SmallOrder_Returns10Percent()
        {
            var calculator = new DiscountCalculator();

            var order = new Order
            {
                Customer = new Customer { IsVip = true },
                Items = new List<OrderItem>
        {
            new OrderItem { Quantity = 1, UnitPrice = 500, Product = new Product { Price = 500 } }
        }
            };

            var discount = calculator.CalculateDiscount(order);

            Assert.Equal(50, discount); // 10%
        }
    }
}
