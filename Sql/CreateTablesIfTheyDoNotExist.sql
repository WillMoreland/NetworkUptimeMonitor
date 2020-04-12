BEGIN;

CREATE TABLE IF NOT EXISTS uptime_results
(
	uptime_result_id	INTEGER PRIMARY KEY,
	date_time_utc		TEXT	NOT NULL,
	was_up				INTEGER	NOT NULL
);

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

END;
