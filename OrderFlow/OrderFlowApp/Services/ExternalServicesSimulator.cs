using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderFlow.OrderFlowApp.Models;

namespace OrderFlow.OrderFlowApp.Services
{
    public class ExternalServiceSimulator
    {
        private readonly Random _random = new Random();

        public async Task<bool> CheckInventoryAsync(Product product)
        {
            await Task.Delay(_random.Next(500, 1500));
            return _random.Next(0, 2) == 1; // losowo true/false
        }

        public async Task<bool> ValidatePaymentAsync(Order order)
        {
            await Task.Delay(_random.Next(1000, 2000));
            return _random.Next(0, 2) == 1;
        }

        public async Task<decimal> CalculateShippingAsync(Order order)
        {
            await Task.Delay(_random.Next(300, 800));
            return _random.Next(10, 100);
        }
    }
}
