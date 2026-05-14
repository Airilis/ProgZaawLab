using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderFlow.OrderFlowApp.Services;
using System.Net;
using System.Net.Http;
using System.Text;
using Moq;
using Moq.Protected;

namespace OrderFlow.Tests
{
    public class CurrencyServiceTests
    {
        [Fact]
        public async Task GetRateAsync_ValidCurrency_ReturnsRate()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("""
                {
                    "rates": [
                        { "mid": 4.00 }
                    ]
                }
                """, Encoding.UTF8, "application/json")
                });

            var client = new HttpClient(handlerMock.Object);

            var service = new CurrencyService(client);

            // Act
            var rate = await service.GetRateAsync("USD");

            // Assert
            Assert.Equal(4.00m, rate);
        }
        [Fact]
        public async Task GetRateAsync_NonExistingCurrency_ReturnsNull()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            var client = new HttpClient(handlerMock.Object);
            var service = new CurrencyService(client);

            // Act
            var result = await service.GetRateAsync("XXX");

            // Assert
            Assert.Null(result);
        }
        [Fact]
        public async Task GetRateAsync_ServerError_ThrowsCurrencyServiceException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var client = new HttpClient(handlerMock.Object);
            var service = new CurrencyService(client);

            // Act & Assert
            await Assert.ThrowsAsync<CurrencyServiceException>(
                () => service.GetRateAsync("USD")
            );
        }
        [Fact]
        public async Task ConvertAsync_UsdToEur_ReturnsConvertedValue()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri!.ToString().Contains("/USD")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("""
            { "rates": [ { "mid": 4.0 } ] }
            """)
                });

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri!.ToString().Contains("/EUR")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("""
            { "rates": [ { "mid": 5.0 } ] }
            """)
                });

            var client = new HttpClient(handlerMock.Object);
            var service = new CurrencyService(client);

            // Act
            var result = await service.ConvertAsync(100, "USD", "EUR");

            // Assert
            Assert.Equal(80m, result);
            // 100 * 4 / 5 = 80
        }
        [Fact]
        public async Task GetRateAsync_PLN_ReturnsOneWithoutHttpCall()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            var client = new HttpClient(handlerMock.Object);
            var service = new CurrencyService(client);

            // Act
            var result = await service.GetRateAsync("PLN");

            // Assert
            Assert.Equal(1m, result);
        }
        [Fact]
        public async Task GetRateAsync_UsesCorrectUrl()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            HttpRequestMessage? capturedRequest = null;

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
                {
                    capturedRequest = req;
                })
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
            await service.GetRateAsync("USD");

            // Assert
            Assert.Contains("/api/exchangerates/rates/A/USD/", capturedRequest!.RequestUri!.ToString());
        }
    }
}
