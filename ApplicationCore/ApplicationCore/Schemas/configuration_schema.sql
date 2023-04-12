
CREATE TABLE configuration (
	id INTEGER NOT NULL,
	ordering_db_path TEXT NOT NULL,
	companies_db_path TEXT NOT NULL,
	work_orders_db_path TEXT NOT NULL
);

INSERT INTO configuration (id, ordering_db_path, companies_db_path, work_orders_db_path) VALUES (1, './ordering.sqlite', './companies.sqlite', './work_orders.sqlite');
