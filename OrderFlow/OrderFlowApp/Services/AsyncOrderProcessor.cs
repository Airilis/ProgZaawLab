using OrderFlow.OrderFlowApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Services
{
    public class AsyncOrderProcessor
    {
        private readonly ExternalServiceSimulator _service = new ExternalServiceSimulator();

        public async Task ProcessOrderAsync(Order order)
        {
            var stopwatch = Stopwatch.StartNew();

            //  równoległe wywołania
            var inventoryTasks = order.Items.Select(i => _service.CheckInventoryAsync(i.Product));

            var paymentTask = _service.ValidatePaymentAsync(order);
            var shippingTask = _service.CalculateShippingAsync(order);
            var allTasks = inventoryTasks.Select(t => (Task)t).Append((Task)paymentTask).Append((Task)shippingTask);

            await Task.WhenAll(allTasks);

            stopwatch.Stop();

            Console.WriteLine($"Order {order.Id} processed in {stopwatch.ElapsedMilliseconds} ms");
        }

        //  wiele zamówień równolegle (max 3)
        public async Task ProcessMultipleOrdersAsync(List<Order> orders)
        {
            var semaphore = new SemaphoreSlim(3);
            int completed = 0;

            var tasks = orders.Select(async order =>
            {
                await semaphore.WaitAsync();

                try
                {
                    await ProcessOrderAsync(order);

                    int done = Interlocked.Increment(ref completed);
                    Console.WriteLine($"Postęp: {done}/{orders.Count}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        //  wersja sekwencyjna (do porównania)
        public async Task ProcessSequentialAsync(List<Order> orders)
        {
            foreach (var order in orders)
            {
                await ProcessOrderAsync(order);
            }
        }
    }
}
