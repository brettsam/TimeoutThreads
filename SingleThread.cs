using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimeoutThreads
{
    internal class SingleThread
    {
        private static readonly object _lock = new object();
        private static List<Scheduled> _cache = new List<Scheduled>();

        private static void Add(CancellationTokenSource token)
        {
            lock (_lock)
            {
                _cache.Add(new Scheduled
                {
                    Expires = DateTime.Now.AddSeconds(5),
                    Token = token
                });
            }
        }

        private static Scheduled[] GetExpired()
        {
            lock (_lock)
            {
                return _cache.Where(p => p.Expires <= DateTime.Now).ToArray();
            }
        }

        private static void Remove(Scheduled scheduled)
        {
            lock (_lock)
            {
                _cache.Remove(scheduled);
            }
        }

        public static async Task Run()
        {
            bool exit = false;

            Thread t = new Thread(_ =>
            {
                while (!exit)
                {
                    var expiredTokens = GetExpired();

                    foreach (var expiredToken in expiredTokens)
                    {
                        expiredToken.Token.Cancel();
                        Remove(expiredToken);
                    }
                }

                Thread.Sleep(100);
            });

            t.Start();

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 1000; i++)
            {
                tasks.Add(FunctionAsync(i));
                //await Task.Delay(250);
            }

            await Task.WhenAll(tasks);

            exit = true;

            t.Join();
        }

        private class Scheduled
        {
            public DateTime Expires { get; set; }
            public CancellationTokenSource Token { get; set; }
        }

        private static async Task FunctionAsync(int i)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{i}:");

            Stopwatch sw = Stopwatch.StartNew();

            Add(cts);

            // Do something that takes a long time; cancel on timer.
            try
            {
                await Task.Delay(100000, cts.Token);
            }
            catch (OperationCanceledException)
            {
                sb.AppendLine($"  Canceled: {sw.ElapsedMilliseconds}");
            }

            sb.AppendLine($"  Exiting: {sw.ElapsedMilliseconds}");
            Console.WriteLine(sb.ToString());
        }

    }
}
