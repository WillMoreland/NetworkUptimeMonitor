using System;
using System.Collections.Generic;
using System.Linq;
using Uptime.Models;

namespace Uptime
{
    class InMemoryUptimeResultDataStore: IUptimeResultDataStore
    {
        private readonly List<UptimeResult> uptimeResults;

        public InMemoryUptimeResultDataStore()
        {
            uptimeResults = new List<UptimeResult>();
        }

        public List<UptimeResult> GetWithDateRange(
            DateTime startDateTimeUtc,
            DateTime endDateTimeUtc
        )
        {
            return uptimeResults.Where(result =>
                result.DateTimeUtc >= startDateTimeUtc &&
                result.DateTimeUtc <= endDateTimeUtc
            ).ToList();
        }

        public List<UptimeResult> GetWithWasUpStatus(bool wasUp)
        {
            return uptimeResults.Where(result => result.WasUp).ToList();
        }

        public void Save(UptimeResult uptimeResult)
        {
            uptimeResults.Add(uptimeResult);
        }
    }
}
