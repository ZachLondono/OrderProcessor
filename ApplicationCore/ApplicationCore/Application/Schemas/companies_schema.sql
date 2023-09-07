CREATE TABLE customers (
	id BLOB NOT NULL,
	name TEXT NOT NULL,
	order_number_prefix TEXT,
	shipping_method TEXT NOT NULL,
	shipping_contact_id BLOB NOT NULL,
	shipping_address_id BLOB NOT NULL,
	billing_contact_id BLOB NOT NULL,
	billing_address_id BLOB NOT NULL,
	closet_pro_settings_id BLOB NOT NULL,
	working_directory_root TEXT,
	PRIMARY KEY (id),
	FOREIGN KEY (shipping_contact_id) REFERENCES contacts(id),
	FOREIGN KEY (billing_contact_id) REFERENCES contacts(id),
	FOREIGN KEY (closet_pro_settings_id) REFERENCES closet_pro_settings(id)
);

CREATE TABLE closet_pro_settings (
	id BLOB NOT NULL,
	toe_kick_sku TEXT NOT NULL,
	adjustable_shelf_sku TEXT NOT NULL,
	fixed_shelf_sku TEXT NOT NULL,
	l_fixed_shelf_sku TEXT NOT NULL,
	l_adjustable_shelf_sku TEXT NOT NULL,
	l_shelf_radius REAL NOT NULL,
	diagonal_fixed_shelf_sku TEXT NOT NULL,
	diagonal_adjustable_shelf_sku TEXT NOT NULL,
	doweled_drawer_box_material_finish TEXT NOT NULL,
	vertical_panel_bottom_radius REAL NOT NULL
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

	release_invoice INTEGER NOT NULL,
	release_invoice_send_email INTEGER NOT NULL,
	release_invoice_email_recipients TEXT NOT NULL,

	release_packing_list INTEGER NOT NULL,
	release_include_invoice INTEGER NOT NULL,
	release_job_summary INTEGER NOT NULL,
	release_send_email INTEGER NOT NULL,
	release_email_recipients TEXT NOT NULL,

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
INSERT INTO addresses (id, line1, line2, line3, city, state, zip, country) VALUES ('4534920a-8b66-4847-b346-7459d7ff3d7b','15E Easy St','','','Bound Brook','NJ','08805','USA');
INSERT INTO vendors (id, name, phone, logo, address_id, export_db_order, export_mdf_door_order, export_ext_file, release_include_invoice, release_packing_list, release_job_summary, release_send_email, release_email_recipients, release_invoice, release_invoice_send_email, release_invoice_email_recipients)
	VALUES ('a81d759d-5b6c-4053-8cec-55a6c94d609e', 'Metro Cabinet Parts', '', (CAST('' AS BLOB)), '4534920a-8b66-4847-b346-7459d7ff3d7b', 0, 0, 0, 0, 0, 0, 0, '', 0, 0, '');

INSERT INTO addresses (id, line1, line2, line3, city, state, zip, country) VALUES ('45249639-5131-480a-9bce-9e06e0843854','15E Easy St','','','Bound Brook','NJ','08805','USA');
INSERT INTO vendors (id, name, phone, logo, address_id, export_db_order, export_mdf_door_order, export_ext_file, release_include_invoice, release_packing_list, release_job_summary, release_send_email, release_email_recipients, release_invoice, release_invoice_send_email, release_invoice_email_recipients)
	VALUES ('579badff-4579-481d-98cf-0012eb2cc75e', 'Royal Cabinet Company', '', (CAST('' AS BLOB)), '45249639-5131-480a-9bce-9e06e0843854', 0, 0, 0, 0, 0, 0, 0, '', 0, 0, '');

INSERT INTO addresses (id, line1, line2, line3, city, state, zip, country) VALUES ('fd0a640e-fa18-48b8-94e2-0df5ed7b66f0','55 South St','Ste C','','Mount Vernon','NY','10550','USA');
INSERT INTO vendors (id, name, phone, logo, address_id, export_db_order, export_mdf_door_order, export_ext_file, release_include_invoice, release_packing_list, release_job_summary, release_send_email, release_email_recipients, release_invoice, release_invoice_send_email, release_invoice_email_recipients)
	VALUES ('38dc201f-669d-41be-b0f4-adfa4c003a99', 'Hafele America Co.', '', (CAST('' AS BLOB)), 'fd0a640e-fa18-48b8-94e2-0df5ed7b66f0', 0, 0, 0, 0, 0, 0, 0, '', 0, 0, '');

INSERT INTO addresses (id, line1, line2, line3, city, state, zip, country) VALUES ('a20b4c89-b2d8-4903-a37d-321c3df9cca6','','','','','','','');
INSERT INTO vendors (id, name, phone, logo, address_id, export_db_order, export_mdf_door_order, export_ext_file, release_include_invoice, release_packing_list, release_job_summary, release_send_email, release_email_recipients, release_invoice, release_invoice_send_email, release_invoice_email_recipients)
	VALUES ('02a8b183-3c98-46e7-8e2b-b14399dd1ac9', 'Richelieu Hardware Ltd', '', (CAST('' AS BLOB)), 'a20b4c89-b2d8-4903-a37d-321c3df9cca6', 0, 0, 0, 0, 0, 0, 0, '', 0, 0, '');
