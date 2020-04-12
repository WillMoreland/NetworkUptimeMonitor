using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NetworkUptimeMonitor.Models
{
    class PingRequest
    {
        public string TargetIpAddress { get; set; }

        public PingReply PingReply { get; set; }

        private const int PING_TIMEOUT_MS = 5000;

        public static async Task<PingRequest> Send(string targetIpAddress)
        {
            var pingReply = await new Ping().SendPingAsync(targetIpAddress, PING_TIMEOUT_MS);
            return new PingRequest
            {
                TargetIpAddress = targetIpAddress,
                PingReply = pingReply
            };
        }
    }
}
