using System;
using System.Collections.Generic;
using Uptime.Models;

namespace Uptime
{
    public interface IUptimeResultDataStore
    {
        List<UptimeResult> GetUptimeResults(DateTime startDateTimeUtc, DateTime endDateTimeUtc, bool wasUp);

        int GetUptimeResultsCount(DateTime startDateTimeUtc, DateTime endDateTimeUtc, bool wasUp);

        void SaveUptimeResult(UptimeResult uptimeResult);
    }
}
