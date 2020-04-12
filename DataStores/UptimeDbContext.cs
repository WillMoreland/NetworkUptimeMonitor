using Microsoft.EntityFrameworkCore;
using Uptime.Models;

namespace Uptime.DataStores
{
    class UptimeDbContext: DbContext
    {
        public DbSet<UptimeResult> UptimeResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options.UseSqlite("Data Source=uptime-data.db");
    }
}
