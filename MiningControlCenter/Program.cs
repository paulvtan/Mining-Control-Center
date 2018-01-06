using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiningControlCenter
{
    class Program
    {
        public static int SecondsToRefresh = 60 * 5;
        
        static void Main(string[] args)
        {
            while (true)
            {
                ScreenBuilder.GetWelcomeMessage();
                Console.WriteLine("Refresh every " + SecondsToRefresh + " seconds.\n");
                ScreenBuilder.DisplayPerformanceMonitoringStats();
                Helper.StartRefreshTimer(SecondsToRefresh);
                Console.Clear();
            }
        }
    }
}
