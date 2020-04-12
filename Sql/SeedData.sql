	BEGIN;

	CREATE TEMP TABLE _variables(name TEXT PRIMARY KEY, value INTEGER);

	INSERT INTO uptime_results
	(
		date_time_utc,
		was_up
	)
	VALUES
	(
		"2020-04-11T21:32:00Z",
		1
	);

	INSERT INTO _variables (name, value) VALUES ("uptime_result_id", last_insert_rowid());

SELECT value FROM _variables WHERE name = "uptime_result_id";

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
	(SELECT value FROM _variables WHERE name = "uptime_result_id"),
	"2020-04-11T21:32:00Z",
	"1.1.1.1",
	0,
	345
),
(
	(SELECT value FROM _variables WHERE name = "uptime_result_id"),
	"2020-04-11T21:32:00Z",
	"1.0.0.1",
	0,
	567
),
(
	(SELECT value FROM _variables WHERE name = "uptime_result_id"),
	"2020-04-11T21:32:00Z",
	"192.168.1.1",
	0,
	3
);

DROP TABLE _variables;
END;
