using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

namespace OrderFlow.OrderFlowApp.Services
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, decimal> _cache = new();

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal?> GetRateAsync(string currencyCode)
        {
            if (currencyCode == "PLN")
                return 1m;
            var url = $"https://api.nbp.pl/api/exchangerates/rates/A/{currencyCode}/?format=json";
            //CACHE CHECK
            if (_cache.TryGetValue(currencyCode, out var cached))
                return cached;
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                throw new CurrencyServiceException($"NBP API error: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var rate = doc.RootElement
                .GetProperty("rates")[0]
                .GetProperty("mid")
                .GetDecimal();
            //STORE IN CACHE
            _cache[currencyCode] = rate;
            return rate;
        }

        public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency)
                return amount;

            if (fromCurrency == "PLN")
            {
                var rate = await GetRateAsync(toCurrency);
                return amount / rate!.Value;
            }

            if (toCurrency == "PLN")
            {
                var rate = await GetRateAsync(fromCurrency);
                return amount * rate!.Value;
            }

            var fromRate = await GetRateAsync(fromCurrency);
            var toRate = await GetRateAsync(toCurrency);

            return amount * fromRate!.Value / toRate!.Value;
        }
    }
    public class CurrencyServiceException : Exception
    {
        public CurrencyServiceException(string message) : base(message)
        {
        }
    }
}
