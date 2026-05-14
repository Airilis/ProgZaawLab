using Moq;
using Moq.Protected;
using OrderFlow.OrderFlowApp.Models;
using OrderFlow.OrderFlowApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Tests
{
    public class OrderCurrencyConverterTests
    {
        [Fact]
        public async Task ConvertOrderTotalAsync_ReturnsConvertedValue()
        {
            // Arrange
            var currencyMock = new Mock<ICurrencyService>();

            currencyMock
                .Setup(x => x.ConvertAsync(It.IsAny<decimal>(), "PLN", "USD"))
                .ReturnsAsync(400);

            var converter = new OrderCurrencyConverter(currencyMock.Object);

            var order = new Order
            {
                Items = new List<OrderItem>
            {
                new OrderItem { Quantity = 1, UnitPrice = 100 }
            }
            };

            // Act
            var result = await converter.ConvertOrderTotalAsync(order, "USD");

            // Assert
            Assert.Equal(400, result);
        }
        [Fact]
        public async Task ConvertOrderTotalAsync_PassesCorrectOrderTotal()
        {
            // Arrange
            var currencyMock = new Mock<ICurrencyService>();

            decimal capturedAmount = 0;

            currencyMock
                .Setup(x => x.ConvertAsync(It.IsAny<decimal>(), "PLN", "EUR"))
                .Callback<decimal, string, string>((amount, _, _) =>
                {
                    capturedAmount = amount;
                })
                .ReturnsAsync(999);

            var converter = new OrderCurrencyConverter(currencyMock.Object);

            var order = new Order
            {
                Items = new List<OrderItem>
        {
            new OrderItem { Quantity = 2, UnitPrice = 50 }, // 100
            new OrderItem { Quantity = 1, UnitPrice = 25 }  // 25
        }
            };

            // Act
            await converter.ConvertOrderTotalAsync(order, "EUR");

            // Assert
            Assert.Equal(125, capturedAmount);
        }
        [Fact]
        public async Task GetRateAsync_CachesResult_CallsApiOnlyOnce()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            var callCount = 0;

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback(() => callCount++)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("""
            { "rates": [ { "mid": 4.0 } ] }
            """)
                });

            var client = new HttpClient(handlerMock.Object);
            var service = new CurrencyService(client);

            // Act
            var r1 = await service.GetRateAsync("USD");
            var r2 = await service.GetRateAsync("USD");

            // Assert
            Assert.Equal(4.0m, r1);
            Assert.Equal(4.0m, r2);
            Assert.Equal(1, callCount); // 🔥 klucz bonusu
        }
    }
}
