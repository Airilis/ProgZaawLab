using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderFlow.OrderFlowApp.Models;

namespace OrderFlow.OrderFlowApp.Services
{
    public class OrderCurrencyConverter
    {
        private readonly ICurrencyService _currencyService;

        public OrderCurrencyConverter(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        public async Task<decimal> ConvertOrderTotalAsync(Order order, string targetCurrency)
        {
            var total = order.Items.Sum(i => i.TotalPrice);

            return await _currencyService.ConvertAsync(total, "PLN", targetCurrency);
        }
    }
}
