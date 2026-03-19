using OrderFlow.OrderFlowApp.Data;
using OrderFlow.OrderFlowApp.Models;
using OrderFlow.OrderFlowApp.Services;
using System;

class Program
{
    static void Main()
    {
        //ZADANIE 2
        var validator = new OrderValidator();

        // Poprawne zamówienie
        var orderGood = SampleData.Orders[1];
        Console.WriteLine($"Walidacja poprawnego zamówienia {orderGood.Id}:");
        var errorsGood = validator.ValidateAll(orderGood);
        if (errorsGood.Count == 0)
            Console.WriteLine("Brak błędów!");
        else
            errorsGood.ForEach(e => Console.WriteLine($"  - {e}"));

        // Zamówienie łamiące reguły
        var orderBad = new OrderFlow.OrderFlowApp.Models.Order
        {
            Id = 999,
            Customer = SampleData.Customers[1],
            Status = OrderFlow.OrderFlowApp.Models.OrderStatus.Cancelled,
            Items = new System.Collections.Generic.List<OrderFlow.OrderFlowApp.Models.OrderItem>
            {
                new OrderFlow.OrderFlowApp.Models.OrderItem
                {
                    Product = SampleData.Products[0],
                    Quantity = 0
                }
            }
        };
        Console.WriteLine($"\nWalidacja złego zamówienia {orderBad.Id}:");
        var errorsBad = validator.ValidateAll(orderBad);
        if (errorsBad.Count == 0)
            Console.WriteLine("Brak błędów!");
        else
            errorsBad.ForEach(e => Console.WriteLine($"  - {e}"));



        //ZADANIE 3

        var processor = new OrderProcessor(SampleData.Orders);

        // --- Predicate przykłady ---
        var vipOrders = processor.FilterOrders(o => o.Customer.IsVip);
        var bigOrders = processor.FilterOrders(o => o.TotalAmount > 1000);
        var completedOrders = processor.FilterOrders(o => o.Status == OrderStatus.Completed);

        Console.WriteLine("VIP Orders:");
        vipOrders.ForEach(o => Console.WriteLine($"Order {o.Id}, Total: {o.TotalAmount}"));

        // --- Action przykłady ---
        processor.ApplyAction(o => o.Status = OrderStatus.Processing, bigOrders);
        processor.ApplyAction(o => Console.WriteLine($"Processed order {o.Id}"), bigOrders);

        // --- Func<Order,T> przykład ---
        var projections = processor.ProjectOrders(o => new { o.Id, CustomerName = o.Customer.Name, Total = o.TotalAmount });
        foreach (var p in projections)
            Console.WriteLine($"Order {p.Id} for {p.CustomerName}: {p.Total} zł");

        // --- Agregacje ---
        Console.WriteLine($"Sum: {processor.AggregateOrders(os => os.Sum(o => o.TotalAmount))} zł");
        Console.WriteLine($"Average: {processor.AggregateOrders(os => os.Average(o => o.TotalAmount))} zł");
        Console.WriteLine($"Max: {processor.AggregateOrders(os => os.Max(o => o.TotalAmount))} zł");

        // --- Łańcuch: filtruj → sortuj → top N → wypisz ---
        processor.FilterSortTakePrint();


        // ZADANIE 4

        var orders = SampleData.Orders;
        // Wywołanie raportów LINQ
        OrderReports.RunReports(orders);
    }
}