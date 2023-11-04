CREATE TABLE IF NOT EXISTS uptime_results
(
	uptime_result_id	INTEGER PRIMARY KEY,
	date_time_utc		TEXT	NOT NULL,
	was_up				INTEGER	NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_uptime_results_was_up_date_time_utc ON uptime_results(was_up, date_time_utc);

CREATE TABLE IF NOT EXISTS ping_results
(
	ping_result_id		INTEGER PRIMARY KEY,
	uptime_result_id	INTEGER NOT NULL,
	date_time_utc		TEXT	NOT NULL,
	target_ip_address	TEXT	NOT NULL,
	status				INTEGER	NOT NULL,
	round_trip_time		INTEGER	NOT NULL,
	FOREIGN KEY(uptime_result_id) REFERENCES uptime_results(uptime_result_id)
);
CREATE INDEX IF NOT EXISTS idx_ping_results_uptime_result_id ON ping_results(uptime_result_id);
