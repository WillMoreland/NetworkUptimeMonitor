using NetworkUptimeMonitor;

namespace NetworkUptimeMonitorTests
{
    public class TestEmptyStore
    {
        [Fact]
        public void TestNewStoreHasEmptyUpResults()
        {
            var dataStore = new SqliteUptimeResultDataStore(StorageMode.InMemory);

            var result = dataStore.GetUptimeResults(DateTime.MinValue, DateTime.MaxValue, true);

            Assert.Empty(result);
        }

        [Fact]
        public void TestNewStoreHasEmptyDownResults()
        {
            var dataStore = new SqliteUptimeResultDataStore(StorageMode.InMemory);

            var result = dataStore.GetUptimeResults(DateTime.MinValue, DateTime.MaxValue, false);

            Assert.Empty(result);
        }

        [Fact]
        public void TestNewStoreHasZeroUpResultCount()
        {
            var dataStore = new SqliteUptimeResultDataStore(StorageMode.InMemory);

            var result = dataStore.GetUptimeResultsCount(DateTime.MinValue, DateTime.MaxValue, true);

            Assert.Equal(0, result);
        }

        [Fact]
        public void TestNewStoreHasZeroDownResultCount()
        {
            var dataStore = new SqliteUptimeResultDataStore(StorageMode.InMemory);

            var result = dataStore.GetUptimeResultsCount(DateTime.MinValue, DateTime.MaxValue, false);

            Assert.Equal(0, result);
        }
    }
}