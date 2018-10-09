using System.Threading.Tasks;

namespace TimeoutThreads
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // await IndividualTimers.Run();
            // await SingleTimer.Run();
            await SingleThread.Run();
        }
    }
}
