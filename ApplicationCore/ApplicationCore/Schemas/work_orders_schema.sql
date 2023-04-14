CREATE TABLE work_orders (
	id BLOB NOT NULL,
	name TEXT NOT NULL,
	order_id BLOB NOT NULL,
	status INTEGER NOT NULL,
	PRIMARY KEY (id)
);

CREATE TABLE work_order_products (
	work_order_id BLOB NOT NULL,
	product_id BLOB NOT NULL,
	PRIMARY KEY (work_order_id, product_id),
	FOREIGN KEY (work_order_id) REFERENCES work_orders
);
