CREATE TABLE customers (
	id BLOB NOT NULL,
	name TEXT NOT NULL,
	shipping_method TEXT NOT NULL,
	shipping_contact_id BLOB NOT NULL,
	shipping_address_id BLOB NOT NULL,
	billing_contact_id BLOB NOT NULL,
	billing_address_id BLOB NOT NULL,
	PRIMARY KEY (id),
	FOREIGN KEY (shipping_contact_id) REFERENCES contacts(id),
	FOREIGN KEY (billing_contact_id) REFERENCES contacts(id)
);

CREATE TABLE vendors (
	id BLOB NOT NULL,
	name TEXT NOT NULL,
	address_id BLOB NOT NULL,
	phone TEXT NOT NULL,
	logo BLOB NOT NULL,
	
	export_db_order INTEGER NOT NULL,
	export_mdf_door_order INTEGER NOT NULL,
	export_ext_file INTEGER NOT NULL,
	export_output_directory TEXT NOT NULL,

	release_invoice INTEGER NOT NULL,
	release_invoice_output_directory TEXT NOT NULL,
	release_invoice_send_email INTEGER NOT NULL,
	release_invoice_email_recipients TEXT NOT NULL,

	release_packing_list INTEGER NOT NULL,
	release_include_invoice INTEGER NOT NULL,
	release_job_summary INTEGER NOT NULL,
	release_send_email INTEGER NOT NULL,
	release_email_recipients TEXT NOT NULL,
	release_output_directory TEXT NOT NULL,

	email_sender_name TEXT NOT NULL,
	email_sender_email TEXT NOT NULL,
	email_sender_password TEXT NOT NULL,

	PRIMARY KEY (id),
	FOREIGN KEY (address_id) REFERENCES addresses(id)
);

CREATE TABLE allmoxy_ids (
	id INTEGER NOT NULL,
	customer_id BLOB NOT NULL,
	PRIMARY KEY (id),
	FOREIGN KEY (customer_id) REFERENCES customers ON DELETE CASCADE
);

CREATE TABLE addresses (
	id BLOB NOT NULL,
	line1 TEXT NOT NULL,
	line2 TEXT NOT NULL,
	line3 TEXT NOT NULL,
	city TEXT NOT NULL,
	state TEXT NOT NULL,
	zip TEXT NOT NULL,
	country TEXT NOT NULL,
	PRIMARY KEY (id)
);

CREATE TABLE contacts (
	id BLOB NOT NULL,
	name TEXT NOT NULL,
	phone_number TEXT,
	email TEXT,
	PRIMARY KEY (id)
);

-- Initial Data --
INSERT INTO addresses (id, line1, line2, line3, city, state, zip, country) VALUES ('4534920a-8b66-4847-b346-7459d7ff3d7b','','','','','','','');
INSERT INTO vendors (id, name, phone, logo, address_id, export_db_order, export_mdf_door_order, export_ext_file, export_output_directory, release_include_invoice, release_packing_list, release_job_summary, release_send_email, release_email_recipients, release_output_directory, release_invoice, release_invoice_output_directory, release_invoice_send_email, release_invoice_email_recipients, email_sender_name, email_sender_email, email_sender_password)
	VALUES ('a81d759d-5b6c-4053-8cec-55a6c94d609e', 'Metro Cabinet Parts', '', (CAST('' AS BLOB)), '4534920a-8b66-4847-b346-7459d7ff3d7b', 0, 0, 0, '.\Output', 0, 0, 0, 0, '', '.\Output', 0, '.\Output', 0, '', '', '', '');