using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Uptime.Models;

namespace Uptime
{
    class Program
    {
        static void Main(string[] args)
        {
            int pingInterval;

            if (args.Length < 2 || !int.TryParse(args[0], out pingInterval))
            {
                PrintUsage();
                return;
            }

            var ipAddressRegEx = new Regex(
                "^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\." +
                "(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\." +
                "(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\." +
                "(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"
            );
            var targetIps = args.Skip(1);
            foreach (var arg in targetIps)
            {
                if (!ipAddressRegEx.IsMatch(arg))
                {
                    Console.Error.WriteLine($"{arg} does not look like a valid IP address");
                    PrintUsage();
                    return;
                }
            }

            var dataStore = new SqliteUptimeResultDataStore();
            var uptimeMonitorer = new UptimeMonitorer(dataStore);

            Console.WriteLine($"Pinging {string.Join(' ', targetIps)} every {pingInterval}ms");
            Console.WriteLine();
            PrintInstructions();
            _ = uptimeMonitorer.Monitor(targetIps, pingInterval);

            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Y)
                {
                    // Print a summary of today's results
                    var yesterday = DateTime.UtcNow.AddDays(-1);
                    PrintResultsForPeriod(dataStore, yesterday, DateTime.UtcNow);
                }
                else if (key.Key == ConsoleKey.W)
                {
                    // Print a summary of this week's results
                    var lastWeek = DateTime.UtcNow.AddDays(-8);
                    PrintResultsForPeriod(dataStore, lastWeek, DateTime.UtcNow);
                }
                else if (key.Key == ConsoleKey.M)
                {
                    // Print a summary of this month's results
                    var lastMonth = DateTime.UtcNow.AddMonths(-1);
                    PrintResultsForPeriod(dataStore, lastMonth, DateTime.UtcNow);
                }
                else if (key.Key == ConsoleKey.Q)
                {
                    return;
                }
                else
                {
                    Console.Error.WriteLine($"{key.Key} is not a valid input.");
                    PrintInstructions();
                }
            }
        }

        private static void PrintInstructions()
        {
            Console.WriteLine("Press y to print a summary since yesterday");
            Console.WriteLine("Press w to print a summary since a week ago");
            Console.WriteLine("Press m to print a summary since a month ago");
            Console.WriteLine("Press q to quit");
            Console.WriteLine();
        }

        private static void PrintResultsForPeriod(
            IUptimeResultDataStore dataStore,
            DateTime startDateTimeUtc,
            DateTime endDateTimeUtc
        )
        {
            var successfulResultCount = dataStore.GetUptimeResultsCount(
                startDateTimeUtc,
                endDateTimeUtc,
                true
            );
            var failedResults = dataStore.GetUptimeResults(startDateTimeUtc, endDateTimeUtc, false);
            Console.Write(
                $"Since {startDateTimeUtc.ToString("yyyy-MM-ddThh:mm:ssZ")}, there have been " +
                $"{string.Format("{0:n0}", successfulResultCount)} successful checks "
            );
            Console.WriteLine(
                failedResults.Any() ?
                    $"which included the following " +
                    $"{string.Format("{0:n0}", failedResults.Count)} " +
                    "failures:"
                :
                    "and no failures."
            );
            PrintResults(failedResults);
        }

        private static void PrintResults(IEnumerable<UptimeResult> results)
        {
            foreach (var result in results)
            {
                Console.WriteLine(result);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: NetworkUptimeMonitor.exe <interval in ms> <ip address> <ip address> <ip address>...");
            Console.WriteLine("Press the any key to continue...");
            Console.ReadKey(true);
        }
    }
}
