using OrderFlow.OrderFlowApp.Data;
using OrderFlow.OrderFlowApp.Models;
using OrderFlow.OrderFlowApp.Persistence;
using OrderFlow.OrderFlowApp.Services;
using OrderFlow.OrderFlowApp.Watchers;
using System;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
        //LAB 4 ZADANIE 2 i 3 (z poprzednich lab niektóre części zostały skomentowane bo z powodu tej samej nazwy były błędy)
        Console.WriteLine("START APP");

        
        var db = new OrderFlowContext();

        await db.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(db);
        Console.WriteLine("\n=== READ ===");

        var orders = db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .ToList();

        foreach (var o in orders)
        {
            Console.WriteLine(
                $"Order #{o.Id} | {o.Customer.FullName} | {o.Status} | Total: {o.TotalAmount}");
        }

        Console.WriteLine("DB gotowa i zseedowana");
        Console.WriteLine("\n=== CREATE ===");

        var customer = db.Customers.First();
        var product1 = db.Products.First();

        var newOrder = new Order
        {
            CustomerId = customer.Id,
            Status = OrderStatus.New,
            Items = new List<OrderItem>
    {
        new OrderItem
        {
            ProductId = product1.Id,
            Quantity = 2,
            UnitPrice = product1.Price
        }
    }
        };

        db.Orders.Add(newOrder);
        await db.SaveChangesAsync();

        Console.WriteLine($"Dodano zamówienie #{newOrder.Id}");
        Console.WriteLine("\n=== UPDATE ===");

        var orderToUpdate = db.Orders.First();

        orderToUpdate.Status = OrderStatus.Processing;
        orderToUpdate.Notes = "Zaktualizowane przez CRUD";

        await db.SaveChangesAsync();

        Console.WriteLine($"Zaktualizowano zamówienie #{orderToUpdate.Id}");
        Console.WriteLine("\n=== DELETE ===");

        var toDelete = db.Orders.FirstOrDefault(o => o.Status == OrderStatus.Cancelled);

        if (toDelete != null)
        {
            db.Orders.Remove(toDelete);
            await db.SaveChangesAsync();

            Console.WriteLine($"Usunięto zamówienie #{toDelete.Id}");
        }
        else
        {
            Console.WriteLine("Brak anulowanych zamówień do usunięcia");

            Console.WriteLine("\n=== VIP ORDERS > 1000 ===");

            var vipOrders = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .AsEnumerable()
                .Where(o => o.Customer.IsVip)
                .Where(o => o.Items.Sum(i => i.UnitPrice * i.Quantity) > 1000)
                .ToList();

            foreach (var o in vipOrders)
            {
                Console.WriteLine($"Order #{o.Id} | VIP: {o.Customer.FullName} | Total: {o.TotalAmount}");
            }

            Console.WriteLine("\n=== CUSTOMER RANKING ===");

            var ranking = db.Customers
                .Select(c => new
                {
                    c.FullName,
                    Total = c.Orders.SelectMany(o => o.Items)
                                     .Sum(i => i.UnitPrice * i.Quantity)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            foreach (var r in ranking)
            {
                Console.WriteLine($"{r.FullName} => {r.Total}");
            }

            Console.WriteLine("\n=== AVERAGE ORDER VALUE ===");

            var avg = db.Orders
                .Include(o => o.Customer)
                .GroupBy(o => o.Customer.FullName)
                .Select(g => new
                {
                    Customer = g.Key,
                    Avg = g.Average(o => o.Items.Sum(i => i.UnitPrice * i.Quantity))
                })
                .ToList();

            foreach (var a in avg)
            {
                Console.WriteLine($"{a.Customer} => {a.Avg}");
            }

            Console.WriteLine("\n=== NEVER ORDERED PRODUCTS ===");

            var neverOrdered = db.Products
                .Where(p => !p.OrderItems.Any())
                .ToList();

            foreach (var p in neverOrdered)
            {
                Console.WriteLine(p.Name);
            }

            Console.WriteLine("\n=== DYNAMIC QUERY ===");

            OrderStatus? statusFilter = OrderStatus.New;
            decimal minTotal = 100;

            var query = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .AsQueryable();

            if (statusFilter != null)
                query = query.Where(o => o.Status == statusFilter);

            query = query.Where(o => o.Items.Sum(i => i.UnitPrice * i.Quantity) >= minTotal);

            var result = query.ToList();

            foreach (var o in result)
            {
                Console.WriteLine($"Order #{o.Id} | {o.Status} | {o.TotalAmount}");
            }

            //LAB 1 ZADANIE 2
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

           /* var processor = new OrderProcessor(SampleData.Orders);

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
            var projections = processor.ProjectOrders(o => new { o.Id, CustomerName = o.Customer.FullName, Total = o.TotalAmount });
            foreach (var p in projections)
                Console.WriteLine($"Order {p.Id} for {p.CustomerName}: {p.Total} zł");

            // --- Agregacje ---
            Console.WriteLine($"Sum: {processor.AggregateOrders(os => os.Sum(o => o.TotalAmount))} zł");
            Console.WriteLine($"Average: {processor.AggregateOrders(os => os.Average(o => o.TotalAmount))} zł");
            Console.WriteLine($"Max: {processor.AggregateOrders(os => os.Max(o => o.TotalAmount))} zł");

            // --- Łańcuch: filtruj → sortuj → top N → wypisz ---
            processor.FilterSortTakePrint();
            */

            // ZADANIE 4

            //var orders = SampleData.Orders;
            // Wywołanie raportów LINQ
            //OrderReports.RunReports(orders);

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

            // LAB 3 KONTYNUACJA

            // ZADANIE 1

            Console.WriteLine("\n=== PERSISTENCE TEST ===");

            var repo = new OrderFlow.OrderFlowApp.Persistence.OrderRepository();

            string jsonPath = "data/orders.json";
            string xmlPath = "data/orders.xml";

            // zapis
            await repo.SaveToJsonAsync(SampleData.Orders, jsonPath);
            await repo.SaveToXmlAsync(SampleData.Orders, xmlPath);

            Console.WriteLine("Zapisano JSON i XML");

            // symulacja "czyszczenia pamięci"
            var empty = new List<Order>();

            // odczyt
            var loadedJson = await repo.LoadFromJsonAsync(jsonPath);
            var loadedXml = await repo.LoadFromXmlAsync(xmlPath);

            // porównanie
            Console.WriteLine($"\nJSON count: {loadedJson.Count}");
            Console.WriteLine($"XML count: {loadedXml.Count}");

            Console.WriteLine($"JSON total: {loadedJson.Sum(o => o.TotalAmount)}");
            Console.WriteLine($"XML total: {loadedXml.Sum(o => o.TotalAmount)}");

            //ZADANIE 2

            Console.WriteLine("\n=== XML REPORT ===");
            var reportBuilder = new OrderFlow.OrderFlowApp.Persistence.XmlReportBuilder();

            string reportPath = "data/report.xml";

            // budowa raportu
            var report = reportBuilder.BuildReport(SampleData.Orders);

            // zapis
            await reportBuilder.SaveReportAsync(report, reportPath);

            Console.WriteLine("Raport zapisany!");

            // odczyt: zamówienia > 1000
            var highValueOrders = await reportBuilder.FindHighValueOrderIdsAsync(reportPath, 1000m);

            Console.WriteLine("\nZamówienia powyżej 1000:");
            foreach (var id in highValueOrders)
            {
                Console.WriteLine($"Order ID: {id}");
            }

            //ZADANIE 3
            Console.WriteLine("\n=== INBOX WATCHER ===");

            var pipelineWatcher = new OrderFlow.OrderFlowApp.Services.OrderPipeline();

            // podpinamy eventy (te same co wcześniej!)
            pipelineWatcher.StatusChanged += OnStatusChanged;
            pipelineWatcher.StatusChanged += SendEmailSimulation;
            pipelineWatcher.StatusChanged += UpdateStatistics;
            pipelineWatcher.ValidationCompleted += OnValidationCompleted;

            var inboxPath = Path.Combine(AppContext.BaseDirectory, "inbox");
            using var watcher = new InboxWatcher(inboxPath, pipelineWatcher); Console.WriteLine("Naciśnij ENTER aby zakończyć...");
            Console.ReadLine();

            //LAB 4
            Console.WriteLine("\n=== TRANSACTION TEST ===");

            await ProcessOrderAsync(db, db.Orders.First().Id);

        }
        static async Task ProcessOrderAsync(OrderFlowContext db, int orderId)
        {
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var order = db.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefault(o => o.Id == orderId);

                if (order == null)
                    throw new Exception("Order not found");

                Console.WriteLine($"Processing order #{order.Id}");

                order.Status = OrderStatus.Processing;

                foreach (var item in order.Items)
                {
                    if (item.Product.Stock < item.Quantity)
                        throw new Exception($"Brak stock dla {item.Product.Name}");

                    item.Product.Stock -= item.Quantity;
                }

                order.Status = OrderStatus.Completed;

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                Console.WriteLine($"Order #{order.Id} COMPLETED");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"ROLLBACK: {ex.Message}");
            }
        }

    }
}