using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Uptime.Models;

namespace Uptime
{
    class UptimeMonitorer
    {
        private readonly IUptimeResultDataStore uptimeResultDataStore;

        public UptimeMonitorer(IUptimeResultDataStore uptimeResultDataStore)
        {
            this.uptimeResultDataStore = uptimeResultDataStore;
        }

        public async Task Monitor(IEnumerable<string> targetAddresses, int monitorIntervalMs)
        {
            if (targetAddresses is null)
            {
                throw new System.ArgumentNullException(nameof(targetAddresses));
            }

            var tasks = new List<Task>();
            while (true)
            {
                _ = PingAddresses(targetAddresses);
                await Task.Delay(monitorIntervalMs);
                tasks.RemoveAll(task => task.IsCompleted);
            }
        }

        private async Task PingAddresses(IEnumerable<string> targetAddresses)
        {
            const int PING_TIMEOUT_MS = 5000;
            var pingRequests = targetAddresses.Select(targetAddress =>
                new Ping().SendPingAsync(targetAddress, PING_TIMEOUT_MS)).ToList();
            var pingReplies = await Task.WhenAll(pingRequests);

            var pingResults = pingReplies.Select(pingReply =>
                new PingResult(pingReply.Address.ToString(), pingReply.Status, (int)pingReply.RoundtripTime));
            var uptimeCheckResult = new UptimeResult
            {
                PingResults = pingResults.ToList(),
            };
            uptimeResultDataStore.Save(uptimeCheckResult);
        }
    }
}
