using OrderFlow.OrderFlowApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OrderFlow.OrderFlowApp.Persistence
{
    public class XmlReportBuilder
    {
        public XDocument BuildReport(IEnumerable<Order> orders)
        {
            var now = DateTime.Now;
            var totalOrders = orders.Count();
            var totalRevenue = orders.Sum(o => o.TotalAmount);

            //grupowanie po statusie

            var byStatus = orders.GroupBy(o => o.Status).Select(g =>
                new XElement("status",
                    new XAttribute("name", g.Key.ToString()),
                    new XAttribute("count", g.Count()),
                    new XAttribute("revenue", g.Sum(o => o.TotalAmount))
                )
            );

            //grupowanie po kliencie

            var byCustomer = orders.GroupBy(o => o.Customer).Select(g =>
                 new XElement("customer",
                    new XAttribute("id", g.Key.Id),
                    new XAttribute("name", g.Key.Name),
                    new XAttribute("isVip", g.Key.IsVip),

                    new XElement("orderCount", g.Count()),
                    new XElement("totalSpent", g.Sum(o => o.TotalAmount)),

                    new XElement("orders", g.Select(o =>
                         new XElement("orderRef",
                              new XAttribute("id", o.Id),
                              new XAttribute("total", o.TotalAmount)))
                    )
                 )
            );
            var doc = new XDocument(
                 new XElement("report",
                     new XAttribute("generated", now),

                     new XElement("summary",
                         new XAttribute("totalOrders", totalOrders),
                         new XAttribute("totalRevenue", totalRevenue)
                     ),

                     new XElement("byStatus", byStatus),

                     new XElement("byCustomer", byCustomer)
                 )
            );

            return doc;
        }
        public async Task SaveReportAsync(XDocument report, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await using FileStream fs = new FileStream(path, FileMode.Create);
            await Task.Run(() => report.Save(fs));
        }
        public async Task<IEnumerable<int>> FindHighValueOrderIdsAsync(string reportPath, decimal threshold)
        {
            if (!File.Exists(reportPath))
                return Enumerable.Empty<int>();

            await using FileStream fs = new FileStream(reportPath, FileMode.Open);
            var doc = await Task.Run(() => XDocument.Load(fs));
            var result = doc.Descendants("orderRef").Where(x => (decimal)x.Attribute("total")! > threshold).Select(x => (int)x.Attribute("id")!);
            return result;
        }
    }
}
