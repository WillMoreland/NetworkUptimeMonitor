using NetworkUptimeMonitor.Models;
using System.Collections.Generic;
using System.Linq;
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
            var pingRequestTasks = targetAddresses.Select(targetAddress =>
                PingRequest.Send(targetAddress));
            var pingRequests = await Task.WhenAll(pingRequestTasks);

            var pingResults = pingRequests.Select(pingRequest =>
                new PingResult(
                    pingRequest.TargetIpAddress,
                    pingRequest.PingReply.Status,
                    (int)pingRequest.PingReply.RoundtripTime
                )
            );
            var uptimeCheckResult = new UptimeResult 
            {
                PingResults = pingResults.ToList(),
            };
            uptimeResultDataStore.SaveUptimeResult(uptimeCheckResult);
        }
    }
}
