using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Models
{
    public enum OrderStatus
    {
        New,
        Validated,
        Processing,
        Completed,
        Cancelled
    }
}
