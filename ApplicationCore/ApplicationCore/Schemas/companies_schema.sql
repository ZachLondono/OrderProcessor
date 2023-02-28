CREATE TABLE companies (
	id BLOB NOT NULL,
	name TEXT NOT NULL,
	PRIMARY KEY (id)
);

CREATE TABLE customer (
	company_id BLOB NOT NULL,
	shipping_method TEXT NOT NULL,
	shipping_contact_id BLOB NOT NULL,
	shipping_address_id BLOB NOT NULL,
	billing_contact_id BLOB NOT NULL,
	billing_address_id BLOB NOT NULL,
	PRIMARY KEY (company_id),
	FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE CASCADE
);

CREATE TABLE vendor (
	company_id BLOB NOT NULL,
	address_id BLIB NOT NULL,
	
	export_db_order INTEGER NOT NULL,
	export_mdf_door_order INTEGER NOT NULL,
	export_ext_file INTEGER NOT NULL,
	eport_output_directory TEXT NOT NULL,

	release_invoice INTEGER NOT NULL,
	release_packing_list INTEGER NOT NULL,
	release_job_summary INTEGER NOT NULL,
	release_send_email INTEGER NOT NULL,
	release_email_recipients INTEGER NOT NULL,
	release_output_directory TEXT NOT NULL,

	PRIMARY KEY (company_id),
	FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE CASCADE
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