using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Services
{
    public interface ICurrencyService
    {
        Task<decimal?> GetRateAsync(string currencyCode);
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
    }
}
