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
            _ = uptimeMonitorer.Monitor(targetIps, pingInterval);

            while (true)
            {
                Console.WriteLine("Press s to print a summary of today's results");
                Console.WriteLine("Press t to print today's results");
                Console.WriteLine("Press u to print results where connection was not up");
                Console.WriteLine("Press q to quit");
                var key = Console.ReadKey();
                Console.WriteLine();
                if (key.Key == ConsoleKey.S)
                {
                    var startDateTime = DateTime.UtcNow.AddDays(-1);
                    var endDateTime = DateTime.UtcNow;
                    var results = dataStore.GetWithDateRange(startDateTime, endDateTime);

                    var numSuccessfulResults = results.Where(r => r.WasUp).Count();
                    var failedResults = results.Where(r => !r.WasUp);
                    Console.Write($"Since {startDateTime.ToString("o")}, there have been " +
                        $"{numSuccessfulResults} successful checks");
                    Console.WriteLine(failedResults.Any() ? " which included the following failures:" : ".");
                    PrintResults(failedResults);
                }
                else if (key.Key == ConsoleKey.T)
                {
                    var results = dataStore.GetWithDateRange(
                        DateTime.UtcNow.AddDays(-1),
                        DateTime.UtcNow
                    );
                    PrintResults(results);
                }
                if (key.Key == ConsoleKey.U)
                {
                    var results = dataStore.GetWithWasUpStatus(false);
                    if (results.Count == 0)
                    {
                        Console.WriteLine("There have been no recorded results where a ping request was unsuccessful");
                    }
                    PrintResults(results);
                }
                else if (key.Key == ConsoleKey.Q)
                {
                    return;
                }
                Console.WriteLine();
            }
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
            Console.ReadKey();
        }
    }
}
