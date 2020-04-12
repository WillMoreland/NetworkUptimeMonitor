using System;
using System.Net.NetworkInformation;

namespace Uptime.Models
{
    public class PingResult
    {
        public int Id { get; set; }

        public DateTime DateTimeUtc { get; set; }

        public string TargetIpAddress { get; set; }

        public IPStatus Status { get; set; }

        public int RoundTripTime { get; set; }

        public PingResult() { }

        public PingResult(string targetIpAddress, IPStatus status, int roundTripTime)
        {
            DateTimeUtc = DateTime.UtcNow;
            TargetIpAddress = targetIpAddress;
            Status = status;
            RoundTripTime = roundTripTime;
        }
    }
}
