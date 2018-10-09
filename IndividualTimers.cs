using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimeoutThreads
{
    internal class IndividualTimers
    {
        public static async Task Run()
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 1000; i++)
            {
                tasks.Add(FunctionAsync(i));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task FunctionAsync(int i)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{i}:");

            // Start a timer
            var t = new System.Timers.Timer(5000)
            {
                AutoReset = false
            };

            Stopwatch sw = Stopwatch.StartNew();

            t.Elapsed += (s, e) =>
            {
                cts.Cancel();
            };

            t.Start();

            // Do something that takes a long time; cancel on timer.
            try
            {
                await Task.Delay(100000, cts.Token);
            }
            catch (OperationCanceledException)
            {
                sb.AppendLine($"  Canceled: {sw.ElapsedMilliseconds}");
            }

            // Stop the timer.
            t.Stop();
            t.Dispose();
            sb.AppendLine($"  Exiting: {sw.ElapsedMilliseconds}");
            Console.WriteLine(sb.ToString());
        }
    }
}
