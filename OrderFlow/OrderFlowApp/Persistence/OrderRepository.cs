using OrderFlow.OrderFlowApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OrderFlow.OrderFlowApp.Persistence
{
    public class OrderRepository
    {
        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // JSON

        public async Task SaveToJsonAsync(IEnumerable<Order> orders, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await using FileStream fs = new FileStream(path, FileMode.Create);
            await JsonSerializer.SerializeAsync(fs, orders, _jsonOptions);
        }
        public async Task<List<Order>> LoadFromJsonAsync(string path)
        {
            if (!File.Exists(path))
                return new List<Order>();

            await using FileStream fs = new FileStream(path, FileMode.Open);
            var orders = await JsonSerializer.DeserializeAsync<List<Order>>(fs, _jsonOptions);
            return orders ?? new List<Order>();
        }

        // XML

        public async Task SaveToXmlAsync(IEnumerable<Order> orders, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var serializer = new XmlSerializer(typeof(List<Order>));
            await using FileStream fs = new FileStream(path, FileMode.Create);
            serializer.Serialize(fs, orders.ToList());
        }
        public async Task<List<Order>> LoadFromXmlAsync(string path)
        {
            if (!File.Exists(path))
                return new List<Order>();
            var serializer = new XmlSerializer(typeof(List<Order>));
            await using FileStream fs = new FileStream(path, FileMode.Open);
            return (List<Order>)serializer.Deserialize(fs)!;
        }

    }
}
