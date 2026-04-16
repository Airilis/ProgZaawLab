using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace OrderFlow.OrderFlowApp.Models
{
    public class Order
    {
        [XmlAttribute] // XML
        public int Id { get; set; }
        [JsonPropertyName("customer")] // JSON custom name
        [XmlElement("customer")]       // XML custom element name
        public Customer Customer { get; set; } = null!;
        [XmlElement("items")]
        [JsonInclude]
        private List<OrderItem> _items = new();
        public List<OrderItem> Items
        {
            get => _items;
            set => _items = value ?? new List<OrderItem>();
        }
        [XmlElement("status")]
        public OrderStatus Status { get; set; } = OrderStatus.New;
        [XmlIgnore]        // ignorowane w XML
        [JsonIgnore]       // ignorowane w JSON
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    }
}
