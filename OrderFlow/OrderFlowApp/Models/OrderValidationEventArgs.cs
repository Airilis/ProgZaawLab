using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Models
{
    public class OrderValidationEventArgs : EventArgs
    {
        public Order Order { get; set; }
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; }

        public OrderValidationEventArgs(Order order, bool isValid, List<string> errors)
        {
            Order = order;
            IsValid = isValid;
            Errors = errors;
        }
    }
}
