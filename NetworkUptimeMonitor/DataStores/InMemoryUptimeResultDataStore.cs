using System;
using System.Collections.Generic;
using System.Linq;
using Uptime.Models;

namespace Uptime
{
    class InMemoryUptimeResultDataStore : IUptimeResultDataStore
    {
        private readonly List<UptimeResult> uptimeResults;

        public InMemoryUptimeResultDataStore()
        {
            uptimeResults = new List<UptimeResult>();
        }

        public List<UptimeResult> GetUptimeResults(
            DateTime startDateTimeUtc,
            DateTime endDateTimeUtc,
            bool wasUp
        )
        {
            return uptimeResults.Where(result =>
                result.DateTimeUtc >= startDateTimeUtc &&
                result.DateTimeUtc <= endDateTimeUtc &&
                result.WasUp == wasUp
            ).ToList();
        }

        public int GetUptimeResultsCount(
           DateTime startDateTimeUtc,
           DateTime endDateTimeUtc,
           bool wasUp
        )
        {
            return GetUptimeResults(startDateTimeUtc, endDateTimeUtc, wasUp).Count;
        }

        public void SaveUptimeResult(UptimeResult uptimeResult)
        {
            uptimeResults.Add(uptimeResult);
        }
    }
}
