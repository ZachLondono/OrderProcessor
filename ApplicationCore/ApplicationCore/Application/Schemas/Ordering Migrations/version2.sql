﻿CREATE TABLE supplies (
	id BLOB NOT NULL,
	order_id BLOB NOT NULL,
	qty INTEGER NOT NULL,
	description TEXT NOT NULL,
	PRIMARY KEY (id),
	FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
);

CREATE TABLE drawer_slides (
	id BLOB NOT NULL,
	order_id BLOB NOT NULL,
	qty INTEGER NOT NULL,
	style TEXT NOT NULL,
	length REAL NOT NULL,
	PRIMARY KEY (id),
	FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
);

CREATE TABLE hanging_rails (
	id BLOB NOT NULL,
	order_id BLOB NOT NULL,
	qty INTEGER NOT NULL,
	finish TEXT NOT NULL,
	length REAL NOT NULL,
	PRIMARY KEY (id),
	FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
);