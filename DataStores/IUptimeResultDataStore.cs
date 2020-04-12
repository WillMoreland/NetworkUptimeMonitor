using System;
using System.Collections.Generic;
using Uptime.Models;

namespace Uptime
{
    public interface IUptimeResultDataStore
    {
       List<UptimeResult> GetWithDateRange(DateTime startDateTimeUtc, DateTime endDateTimeUtc);

       List<UptimeResult> GetWithWasUpStatus(bool wasUp);

       void Save(UptimeResult uptimeResult);
    }
} 
