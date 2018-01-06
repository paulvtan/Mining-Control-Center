using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiningControlCenter
{
    public class ScreenBuilder
    {
        public static void GetWelcomeMessage()
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;
            string message = "Mining Control Center";
            Console.WriteLine(message);
            Helper.Underline(message, "=");
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void DisplayPerformanceMonitoringStats()
        {
            new Connector();
        }

        public static void DisplayMainMenu()
        {
            //TO DO
        }

        public static async void DisplayDwarfUserStatsAsync(DwarfUserInformation dwarfUserInformation)
        {
            WriteInColor("DwarfPool Ethereum\n", ConsoleColor.Red);
            WriteInColor("Performance Monitoring\n", ConsoleColor.Cyan);
            Console.WriteLine();
            Console.Write("Total sent hashrate: ");
            WriteInColor(dwarfUserInformation.DwarfUser.Total_hashrate + " Mh/s.\n", ConsoleColor.Green);
            Console.Write("Total calculated hashrate: ");
            WriteInColor(dwarfUserInformation.DwarfUser.Total_hashrate_calculated + " Mh/s.\n\n", ConsoleColor.Green);
            WriteInColor("Current Running Rig\n", ConsoleColor.Cyan);
            Console.WriteLine(String.Format("Total: {0}", dwarfUserInformation.NumberOfRig));
            Console.WriteLine();
            int number = 1;
            foreach (var worker in dwarfUserInformation.Workers)
            {
                string workerName = number + ". " + worker.Worker;
                Console.WriteLine(workerName);
                Helper.Underline(workerName, "-");
                Console.Write("Current Hashrate: ");
                WriteInColor(worker.Hashrate + " Mh/s.\n", ConsoleColor.Green);
                Console.Write("Last Work Submit: ");
                DateTime convertedDate = DateTime.SpecifyKind(DateTime.Parse(worker.Last_submit), DateTimeKind.Utc);
                WriteInColor(convertedDate.ToString("dddd, dd MMMM yyyy HH:mm:ss", CultureInfo.CreateSpecificCulture("en-US")) + "\n", ConsoleColor.Yellow);
                number += 1;
            }
            Console.Write("Payout Limit: ");
            WriteInColor(dwarfUserInformation.PayOutLimit + " ETH.\n\n", ConsoleColor.Red);
            WriteInColor("Mining in Progress\n\n", ConsoleColor.Cyan);
            Console.Write("Amount mined: ");
            WriteInColor(dwarfUserInformation.DwarfUser.Wallet_balance, ConsoleColor.Magenta);
            Console.Write(" / " + dwarfUserInformation.PayOutLimit + "\n");
            Task<int> task = ShowMiningProgression(double.Parse(dwarfUserInformation.DwarfUser.Wallet_balance), dwarfUserInformation.PayOutLimit);
            int result = await task;
        }


        public static void WriteInColor<T>(T message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
        }

        public static async Task<int> ShowMiningProgression(double currentAmount, double payOutLimit)
        {

            using (var progress = new Helper.ProgressBar())
            {
                double percentage = (currentAmount / payOutLimit) * 100;
                if (percentage <= 100)
                {
                    await Task.Run(() =>
                    {
                        var t = Program.SecondsToRefresh;
                        while (t > 0)
                        {
                            progress.Report((double)percentage / 100);
                            Thread.Sleep(1000);
                            t -= 1;
                        }
                    });
                } else
                {
                    Console.WriteLine("Successfully Mined.");
                }
            }
            Console.WriteLine("Refreshing...");
            return 1;
        }
    }
}



