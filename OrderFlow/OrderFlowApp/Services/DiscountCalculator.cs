using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderFlow.OrderFlowApp.Models;

namespace OrderFlow.OrderFlowApp.Services
{
    public class DiscountCalculator
    {
        public decimal CalculateDiscount(Order order)
        {
            decimal total = order.Items.Sum(i => i.UnitPrice * i.Quantity);
            decimal discountPercent = 0;

            if (order.Customer.IsVip)
                discountPercent += 10;

            if (total > 1000)
                discountPercent += 5;

            if (total > 5000 && order.Customer.IsVip)
                discountPercent += 5;

            if (discountPercent > 25)
                discountPercent = 25;

            return total * discountPercent / 100;
        }
    }
}
