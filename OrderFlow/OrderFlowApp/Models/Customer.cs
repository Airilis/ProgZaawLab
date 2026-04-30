using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Models
{
    public class Customer
    {
        public int Id { get; set; }
        //public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsVip { get; set; } = false;
        public string? Email { get; set; }
        // nawigacja
        public List<Order> Orders { get; set; } = new();
    }
}
