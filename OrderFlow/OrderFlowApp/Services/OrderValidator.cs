using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OrderFlow.OrderFlowApp.Services
{
    using Models;
    // Delegat do walidacji
    public delegate bool ValidationRule(Order order, out string errorMessage);

    public class OrderValidator
    {
        // Lista reguł opartych na delegacie
        private List<ValidationRule> rules = new List<ValidationRule>();

        // Lista reguł opartych na Func<Order,bool>
        private List<Func<Order, (bool IsValid, string Error)>> funcRules = new List<Func<Order, (bool, string)>>();

        public OrderValidator()
        {
            // Dodaj reguły named methods
            rules.Add(HasItems);
            rules.Add(TotalAmountUnderLimit);
            rules.Add(QuantitiesPositive);

            // Dodaj reguły jako lambdy
            funcRules.Add(o => (o.Status != OrderStatus.Cancelled, "Order is cancelled"));
            funcRules.Add(o => (o.TotalAmount <= 10000, "Order total amount exceeds 10 000"));
        }

        // Named methods
        public bool HasItems(Order order, out string errorMessage)
        {
            if (order.Items.Count == 0)
            {
                errorMessage = "Order has no items.";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }

        public bool TotalAmountUnderLimit(Order order, out string errorMessage)
        {
            if (order.TotalAmount > 5000)
            {
                errorMessage = "Total amount exceeds 5000.";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }

        public bool QuantitiesPositive(Order order, out string errorMessage)
        {
            foreach (var item in order.Items)
            {
                if (item.Quantity <= 0)
                {
                    errorMessage = $"Item {item.Product.Name} has non-positive quantity.";
                    return false;
                }
            }
            errorMessage = string.Empty;
            return true;
        }

        // Metoda łącząca wszystkie walidacje
        public List<string> ValidateAll(Order order)
        {
            var errors = new List<string>();

            // Delegates
            foreach (var rule in rules)
            {
                if (!rule(order, out var err) && !string.IsNullOrEmpty(err))
                    errors.Add(err);
            }

            // Func<Order,bool>
            foreach (var funcRule in funcRules)
            {
                var (isValid, err) = funcRule(order);
                if (!isValid && !string.IsNullOrEmpty(err))
                    errors.Add(err);
            }

            return errors;
        }
    }
}

