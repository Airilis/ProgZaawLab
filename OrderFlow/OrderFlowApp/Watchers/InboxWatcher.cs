using OrderFlow.OrderFlowApp.Models;
using OrderFlow.OrderFlowApp.Persistence;
using OrderFlow.OrderFlowApp.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Watchers
{
    public class InboxWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly OrderPipeline _pipeline;
        private readonly SemaphoreSlim _semaphore = new(2);

        public InboxWatcher(string path, OrderPipeline pipeline)
        {
            Directory.CreateDirectory(path);

            _pipeline = pipeline;

            _watcher = new FileSystemWatcher(path, "*.json")
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
            };

            _watcher.Created += OnCreated;
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            await _semaphore.WaitAsync();

            try
            {
                Console.WriteLine($"[WATCHER] Wykryto plik: {e.Name}");

                //  retry na lock pliku 
                string json = await ReadFileWithRetryAsync(e.FullPath);

                var repo = new OrderRepository();
                var orders = await repo.LoadFromJsonAsync(e.FullPath);

                if (orders == null || orders.Count == 0)
                {
                    Console.WriteLine("[WATCHER] Brak zamówień w pliku");
                    return;
                }

                foreach (var order in orders)
                {
                    _pipeline.ProcessOrder(order); // LAB 2 pipeline (sync)
                }

                //  przeniesienie do processed
                var processedDir = Path.Combine(Path.GetDirectoryName(e.FullPath)!, "processed");
                Directory.CreateDirectory(processedDir);

                var destPath = Path.Combine(processedDir, Path.GetFileName(e.FullPath));
                File.Move(e.FullPath, destPath, true);

                Console.WriteLine("[WATCHER] Przeniesiono do processed/");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WATCHER ERROR] {ex.Message}");

                var failedDir = Path.Combine(Path.GetDirectoryName(e.FullPath)!, "failed");
                Directory.CreateDirectory(failedDir);

                var destPath = Path.Combine(failedDir, Path.GetFileName(e.FullPath));

                File.Move(e.FullPath, destPath, true);
                await File.WriteAllTextAsync(destPath + ".error.txt", ex.ToString());
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static async Task<string> ReadFileWithRetryAsync(string path)
        {
            const int maxAttempts = 5;

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    return await File.ReadAllTextAsync(path);
                }
                catch (IOException)
                {
                    await Task.Delay(200);
                }
            }

            throw new IOException($"Nie można odczytać pliku: {path}");
        }

        public void Dispose()
        {
            _watcher.Dispose();
            _semaphore.Dispose();
        }
    }
}