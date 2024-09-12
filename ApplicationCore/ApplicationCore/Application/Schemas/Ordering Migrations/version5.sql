CREATE TABLE product_production_notes (
	id BLOB,
	product_id BLOB,
	value TEXT,
	PRIMARY KEY (id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

ALTER TABLE products DROP COLUMN production_notes;
