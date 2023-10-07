using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Uptime.Models
{
    public class UptimeResult
    {
        public int Id { get; set; }

        public DateTime DateTimeUtc { get; set; }

        public List<PingResult> PingResults { get; set; }

        public bool WasUp {
            get =>
                PingResults.Count > 0 &&
                PingResults.All(pingResult => pingResult.Status == IPStatus.Success);
        }

        public UptimeResult()
        {
            DateTimeUtc = DateTime.UtcNow;
            PingResults = new List<PingResult>();
        }

        public override string ToString()
        {
            var datetimeUtcIso8601 = DateTimeUtc.ToString("yyyy-MM-ddTHH\\:mm\\:ssZ");

            var successIps = PingResults
                .Where(result => result.Status == IPStatus.Success)
                .Select(result => result.TargetIpAddress)
                .ToArray();
            var successString = successIps.Length == 0 ? "" :
                $"Successful: {string.Join(", ", successIps)}";

            var failedIps = PingResults
                .Where(result => result.Status != IPStatus.Success)
                .Select(result => result.TargetIpAddress)
                .ToArray();
            var failedString = failedIps.Length == 0 ? "" :
                $"Failed: {string.Join(", ", failedIps)}";


            return $"{datetimeUtcIso8601} {successString} {failedString}";
        }
    }
}
