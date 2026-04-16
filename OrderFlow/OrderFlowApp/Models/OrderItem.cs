using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Models
{
    public class OrderItem
    {
        [JsonPropertyName("product")]
        public Product Product { get; set; } = null!;
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        public decimal TotalPrice => Product.Price * Quantity;
    }
}
