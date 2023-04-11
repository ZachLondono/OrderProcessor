
CREATE TABLE configuration (
	has_been_configured INTEGER NOT NULL,
	ordering_db_path TEXT NOT NULL,
	companies_db_path TEXT NOT NULL,
	work_orders_db_path TEXT NOT NULL
);

INSERT INTO configuration (has_been_configured, ordering_db_path, companies_db_path) VALUES (0, './ordering.sqlite', './companies.sqlite', './work_orders.sqlite');
