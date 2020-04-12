using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using Uptime.Models;

namespace Uptime
{
    class SqliteUptimeResultDataStore: IUptimeResultDataStore
    {
        private readonly string ConnectionString;
        private readonly object DbLock;

        public SqliteUptimeResultDataStore()
        {
            var dbFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "UptimeSqliteDatabase.db");
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dbFilePath
            };
            ConnectionString = connectionStringBuilder.ConnectionString;
            DbLock = new object();
            EnsureDbExists(dbFilePath);
        }

        public List<UptimeResult> GetUptimeResults(
            DateTime startDateTimeUtc,
            DateTime endDateTimeUtc,
            bool wasUp
        )
        {
            lock (DbLock)
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    const string selectUptimeResultsQuery = @"
                        SELECT
                            uptime_result_id,
                            date_time_utc
                        FROM uptime_results
                        WHERE
                            was_up = @wasUp
                        AND
                            date_time_utc BETWEEN @startDateTimeUtc AND @endDateTimeUtc;
                    ";
                    using (var transaction = connection.BeginTransaction())
                    {
                        var selectUptimeCmd = connection.CreateCommand();
                        selectUptimeCmd.CommandText = selectUptimeResultsQuery;
                        selectUptimeCmd.Parameters.AddWithValue(
                            "@wasUp",
                            wasUp ? 1 : 0
                        );
                        selectUptimeCmd.Parameters.AddWithValue(
                            "@startDateTimeUtc",
                            startDateTimeUtc
                        );
                        selectUptimeCmd.Parameters.AddWithValue(
                            "@endDateTimeUtc",
                            endDateTimeUtc
                        );

                        var uptimeResults = ReadUptimeResultsFromCommand(selectUptimeCmd);
                        AddPingResultsToUptimeResults(connection, uptimeResults);
                        return uptimeResults;
                    }
                }
        }

        public int GetUptimeResultsCount(
            DateTime startDateTimeUtc,
            DateTime endDateTimeUtc,
            bool wasUp
        )
        {
            lock (DbLock)
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    const string query = @"
                        SELECT count(uptime_result_id)
                        FROM uptime_results
                        WHERE
                            was_up = @wasUp
                        AND
                            date_time_utc BETWEEN @startDateTimeUtc AND @endDateTimeUtc;
                    ";
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@wasUp", wasUp ? "1" : "0");
                    cmd.Parameters.AddWithValue("@startDateTimeUtc", startDateTimeUtc);
                    cmd.Parameters.AddWithValue("@endDateTimeUtc", endDateTimeUtc);
                    return (int)(long)cmd.ExecuteScalar();
                }
        }

        public void SaveUptimeResult(UptimeResult uptimeResult)
        {
            lock (DbLock)
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        var uptimeResultId = InsertUptimeResult(connection, uptimeResult);

                        foreach (var pingResult in uptimeResult.PingResults)
                        {
                            InsertPingResult(pingResult, connection, uptimeResultId);
                        }
                        transaction.Commit();
                    }
                }
        }

        private void EnsureDbExists(string dbFilePath)
        {
            if (File.Exists(dbFilePath))
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(dbFilePath));
            File.Create(dbFilePath).Close();
            var sqlFileStream = Assembly
                .GetEntryAssembly()
                .GetManifestResourceStream("NetworkUptimeMonitor.Sql.CreateTablesIfTheyDoNotExist.sql");
            var createTablesSqlFile = new StreamReader(sqlFileStream).ReadToEnd();

            lock (DbLock)
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = createTablesSqlFile;
                    command.ExecuteNonQuery();
                }
        }

        private static void InsertPingResult(
            PingResult pingResult,
            SqliteConnection connection,
            int uptimeResultId
        )
        {
            const string insertPingResultQuery = @"
                INSERT INTO ping_results
                (
                    uptime_result_id,
                    date_time_utc,
                    target_ip_address,
                    status,
                    round_trip_time
                )
                VALUES
                (
                    @uptimeResultId,
                    @DateTimeUtc,
                    @TargetIpAddress,
                    @Status,
                    @RoundTripTime
                );
            ";
            var insertPingCmd = connection.CreateCommand();
            insertPingCmd.CommandText = insertPingResultQuery;
            insertPingCmd.Parameters.AddWithValue(
                "@uptimeResultId",
                uptimeResultId
            );
            insertPingCmd.Parameters.AddWithValue(
                "@DateTimeUtc",
                pingResult.DateTimeUtc
            );
            insertPingCmd.Parameters.AddWithValue(
                "@TargetIpAddress",
                pingResult.TargetIpAddress
            );
            insertPingCmd.Parameters.AddWithValue(
                "@Status",
                (int)pingResult.Status
            );
            insertPingCmd.Parameters.AddWithValue(
                "@RoundTripTime",
                pingResult.RoundTripTime
            );
            insertPingCmd.ExecuteNonQuery();
        }

        
        private List<UptimeResult> ReadUptimeResultsFromCommand(
            SqliteCommand command
        )
        {
            var results = new List<UptimeResult>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var dateTimeUtc = reader.GetString(1);
                    results.Add(new UptimeResult
                    {
                        Id = id,
                        DateTimeUtc = DateTime.Parse(dateTimeUtc)
                    });
                }
            }
            return results;
        }

        private void AddPingResultsToUptimeResults(
            SqliteConnection connection,
            List<UptimeResult> uptimeResults
        )
        {
            var batchSize = 999;
            for (var i = 0; i < uptimeResults.Count; i += batchSize)
            {
                var rangeEnd = Math.Min(batchSize, uptimeResults.Count - i);
                var uptimeResultsBatch = uptimeResults.GetRange(i, rangeEnd);
                var queryParamNames = uptimeResultsBatch.Select(result => $"@Param{result.Id}");
                var selectPingResultsQuery = @$"
                SELECT
                    ping_result_id,
                    uptime_result_id,
                    date_time_utc,
                    target_ip_address,
                    status,
                    round_trip_time
                FROM ping_results
                WHERE uptime_result_id IN ({string.Join(", ", queryParamNames)});
            ";
                var uptimeResultIds = uptimeResultsBatch.Select(result => result.Id);
                using (var transaction = connection.BeginTransaction())
                {
                    var selectUptimeCmd = connection.CreateCommand();
                    selectUptimeCmd.CommandText = selectPingResultsQuery;
                    uptimeResultsBatch.ForEach((result) =>
                {
                    selectUptimeCmd.Parameters.AddWithValue(
                        $"@Param{result.Id}",
                        result.Id.ToString()
                    );
                });

                    using (var reader = selectUptimeCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var pingResultId = reader.GetInt32(0);
                            var uptimeResultId = reader.GetInt32(1);
                            var dateTimeUtc = reader.GetString(2);
                            var targetIpAddress = reader.GetString(3);
                            var status = reader.GetInt32(4);
                            var roundTripTime = reader.GetInt32(5);
                            var pingResult = new PingResult
                            {
                                Id = uptimeResultId,
                                DateTimeUtc = DateTime.Parse(dateTimeUtc),
                                TargetIpAddress = targetIpAddress,
                                Status = (IPStatus)status,
                                RoundTripTime = roundTripTime,
                            };
                            var uptimeResult = uptimeResultsBatch
                                .Where(result => result.Id == uptimeResultId)
                                .FirstOrDefault();
                            uptimeResult.PingResults.Add(pingResult);
                        }
                    }
                }
            }
        }

        private int InsertUptimeResult(
            SqliteConnection connection,
            UptimeResult uptimeResult
        )
        {
            const string insertUptimeResultQuery = @"
                INSERT INTO uptime_results (date_time_utc, was_up)
                VALUES (@DateTimeUtc, @WasUp);
                SELECT last_insert_rowid();
            ";
            var insertUptimeCmd = connection.CreateCommand();
            insertUptimeCmd.CommandText = insertUptimeResultQuery;
            insertUptimeCmd.Parameters.AddWithValue(
                "@DateTimeUtc",
                uptimeResult.DateTimeUtc
            );
            insertUptimeCmd.Parameters.AddWithValue(
                "@WasUp",
                uptimeResult.WasUp ? 1 : 0
            );
            var uptimeResultId = (int)(long)insertUptimeCmd.ExecuteScalar();
            return uptimeResultId;
        }
    }
}
