using OrderFlow.OrderFlowApp.Data;
using OrderFlow.OrderFlowApp.Models;
using OrderFlow.OrderFlowApp.Services;
using System;

class Program
{
    static void OnStatusChanged(object sender, OrderStatusChangedEventArgs e)
    {
        Console.WriteLine($"[STATUS] Order {e.Order.Id}: {e.OldStatus} → {e.NewStatus} ({e.Timestamp:T})");
    }

    static void OnValidationCompleted(object sender, OrderValidationEventArgs e)
    {
        if (e.IsValid)
        {
            Console.WriteLine($"[VALIDATION] Order {e.Order.Id}: OK");
        }
        else
        {
            Console.WriteLine($"[VALIDATION] Order {e.Order.Id}: ERROR");
            e.Errors.ForEach(err => Console.WriteLine($"   - {err}"));
        }
    }

    static void SendEmailSimulation(object sender, OrderStatusChangedEventArgs e)
    {
        if (e.NewStatus == OrderStatus.Completed)
        {
            Console.WriteLine($"[EMAIL] Wysłano email o zakończeniu zamówienia {e.Order.Id}");
        }
    }

    static int processedCount = 0;

    static void UpdateStatistics(object sender, OrderStatusChangedEventArgs e)
    {
        if (e.NewStatus == OrderStatus.Completed)
        {
            processedCount++;
            Console.WriteLine($"[STATS] Przetworzono: {processedCount}");
        }
    }
    static async Task Main()
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

        //Lab 2 Main continuation

        // ZADANIE 1

        Console.WriteLine("\n=== PIPELINE EVENTS ===");

        var pipeline = new OrderFlow.OrderFlowApp.Services.OrderPipeline();

        //  Subskrypcje
        pipeline.StatusChanged += OnStatusChanged;
        pipeline.StatusChanged += SendEmailSimulation;
        pipeline.StatusChanged += UpdateStatistics;

        pipeline.ValidationCompleted += OnValidationCompleted;

        //  Test – kilka zamówień
        foreach (var order in SampleData.Orders.Take(3))
        {
            Console.WriteLine($"\n--- Start processing Order {order.Id} ---");
            pipeline.ProcessOrder(order);
        }

        // ZADANIE 2

        Console.WriteLine("\n=== ASYNC PROCESSING ===");

        var asyncProcessor = new OrderFlow.OrderFlowApp.Services.AsyncOrderProcessor();
        var asyncOrders = SampleData.Orders.Take(6).ToList();

        //  SEKWENCYJNIE
        var sw1 = System.Diagnostics.Stopwatch.StartNew();
        await asyncProcessor.ProcessSequentialAsync(asyncOrders);
        sw1.Stop();

        Console.WriteLine($"Sekwencyjnie: {sw1.ElapsedMilliseconds} ms\n");

        //  RÓWNOLEGLE
        var sw2 = System.Diagnostics.Stopwatch.StartNew();
        await asyncProcessor.ProcessMultipleOrdersAsync(asyncOrders);
        sw2.Stop();

        Console.WriteLine($"Równolegle: {sw2.ElapsedMilliseconds} ms");

        // ZADANIE 3

        //test błędu
        //skomentowany żeby wyjątki nie wydawał i dało się uruchomić normalnie
        /*
        Console.WriteLine("\n=== THREAD SAFETY - BAD ===");

        var statsBad = new OrderFlow.OrderFlowApp.Services.OrderStatistics();

        Parallel.ForEach(SampleData.Orders, order =>
        {
            statsBad.AddOrder(order);
        });

        Console.WriteLine($"TotalProcessed: {statsBad.TotalProcessed}");
        Console.WriteLine($"TotalRevenue: {statsBad.TotalRevenue}");
        */
        Console.WriteLine("\n=== THREAD SAFETY - FIXED ===");

        var statsGood = new OrderFlow.OrderFlowApp.Services.OrderStatisticsSafe();

        Parallel.ForEach(SampleData.Orders, order =>
        {
            statsGood.AddOrder(order);
        });

        Console.WriteLine($"TotalProcessed: {statsGood.TotalProcessed}");
        Console.WriteLine($"TotalRevenue: {statsGood.TotalRevenue}");

        foreach (var kv in statsGood.OrdersPerStatus)
        {
            Console.WriteLine($"{kv.Key}: {kv.Value}");
        }

    }
}