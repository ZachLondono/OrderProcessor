-- Order Tables --

CREATE TABLE orders (
	id BLOB NOT NULL,
	source TEXT NOT NULL,
	number TEXT NOT NULL,
	name TEXT NOT NULL,
	note TEXT NOT NULL,
	working_directory TEXT NOT NULL,
	vendor_id BLOB NOT NULL,
	customer_id BLOB NOT NULL,
	customer_comment TEXT,
	order_date TEXT NOT NULL,
	info TEXT,
	tax REAL NOT NULL,
	price_adjustment REAL NOT NULL,
	rush INTEGER NOT NULL,
	shipping_method TEXT,
	shipping_price REAl,
	shipping_contact TEXT,
	shipping_phone_number TEXT,
	shipping_address_id BLOB NOT NULL,
	invoice_email TEXT,
	billing_phone_number TEXT,
	billing_address_id BLOB NOT NULL,
	PRIMARY KEY (id),
	FOREIGN KEY (shipping_address_id) REFERENCES addresses(id),
	FOREIGN KEY (billing_address_id) REFERENCES addresses(id)
);

CREATE TRIGGER remove_order_addresses AFTER DELETE ON orders
BEGIN
	DELETE FROM addresses WHERE id = OLD.shipping_address_id;
	DELETE FROM addresses WHERE id = OLD.billing_address_id;
END;

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

CREATE TABLE additional_items (
	id BLOB NOT NULL,
	order_id BLOB NOT NULL,
	description TEXT NOT NULL,
	price REAL NOT NULL,
	PRIMARY KEY (id),
	FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
);

CREATE TABLE order_numbers (
	name TEXT NOT NULL,
	number INTEGER NOT NULL,
	PRIMARY KEY (name)
);

INSERT INTO order_numbers (name, number) VALUES ("closet_pro_software", 0);

-- Product Tables --

CREATE TABLE products (
	id BLOB NOT NULL,
	order_id BLOB NOT NULL,
	qty INTEGER NOT NULL,
	unit_price REAL NOT NULL,
	product_number INTEGER NOT NULL,
	room TEXT NOT NULL,
	PRIMARY KEY (id),
	FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
);

CREATE TABLE mdf_door_products (
	product_id BLOB NOT NULL,
	note TEXT NOT NULL,
	height REAL NOT NULL,
	width REAL NOT NULL,
	type INTEGER NOT NULL,
	top_rail REAL NOT NULL,
	bottom_rail REAL NOT NULL,
	left_stile REAL NOT NULL,
	right_stile REAL NOT NULL,
	orientation INTEGER NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (product_id) REFERENCES mdf_door_configs(id)
);

CREATE TRIGGER remove_mdf_door_config AFTER DELETE ON mdf_door_products
BEGIN
	DELETE FROM mdf_door_configs WHERE id = OLD.product_id;
	DELETE FROM mdf_door_openings WHERE product_id = OLD.product_id;
END;

CREATE TABLE dovetail_drawer_products (
	product_id BLOB NOT NULL,
	height REAL NOT NULL,
	width REAL NOT NULL,
	depth REAL NOT NULL,
	note TEXT NOT NULL,
	label_fields TEXT NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (product_id) REFERENCES drawer_box_configs(id)
);

CREATE TRIGGER remove_dovetail_drawer_config AFTER DELETE ON dovetail_drawer_products
BEGIN
	DELETE FROM drawer_box_configs WHERE id = OLD.product_id;
END;

CREATE TABLE doweled_drawer_products (
	product_id BLOB NOT NULL,
	height REAL NOT NULL,
	width REAL NOT NULL,
	depth REAL NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (product_id) REFERENCES doweled_drawer_box_configs(id) ON DELETE CASCADE
);

CREATE TRIGGER remove_doweled_drawer_config AFTER DELETE ON doweled_drawer_products
BEGIN
	DELETE FROM doweled_drawer_box_configs WHERE id = OLD.product_id; 
END;

CREATE TABLE closet_parts (
	product_id BLOB NOT NULL,
	sku TEXT NOT NULL,
	width REAL NOT NULL,
	length REAL NOT NULL,
	material_finish TEXT NOT NULL,
	material_core INTEGER NOT NULL,
	paint_color TEXT,
	painted_side INTEGER NOT NULL,
	edge_banding_finish TEXT NOT NULL,
	comment TEXT NOT NULL,
	parameters TEXT NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

CREATE TABLE custom_drilled_vertical_panels (
	product_id BLOB NOT NULL,
	width REAL NOT NULL,
	length REAL NOT NULL,
	material_finish TEXT NOT NULL,
	material_core INTEGER NOT NULL,
	paint_color TEXT,
	painted_side INTEGER NOT NULL,
	edge_banding_finish TEXT NOT NULL,
	comment TEXT NOT NULL,
	drilling_type INTEGER NOT NULL,
	extend_back REAL NOT NULL,
	extend_front REAL NOT NULL,
	hole_dim_from_bottom REAL NOT NULL,
	hole_dim_from_top REAL NOT NULL,
	trans_hole_dim_from_bottom REAL NOT NULL,
	trans_hole_dim_from_top REAL NOT NULL,
	bottom_notch_depth REAL NOT NULL,
	bottom_notch_height REAL NOT NULL,
	led_channel_off_front REAL NOT NULL,
	led_channel_width REAL NOT NULL,
	led_channel_depth REAL NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

CREATE TABLE cabinets (
	product_id BLOB NOT NULL,
	height REAL NOT NULL,
	width REAL NOT NULL,
	depth REAL NOT NULL,
	box_material_core INTEGER NOT NULL,
	box_material_finish TEXT NOT NULL,
	box_material_finish_type INTEGER NOT NULL,
	finish_material_core INTEGER NOT NULL,
	finish_material_finish TEXT NOT NULL,
	finish_material_finish_type TEXT NOT NULL,
	finish_material_paint TEXT,
	slab_door_core INTEGER,
	slab_door_finish TEXT,
	slab_door_finish_type INTEGER,
	slab_door_paint TEXT,
	edge_banding_finish TEXT NOT NULL,
	left_side_type INTEGER NOT NULL,
	right_side_type INTEGER NOT NULL,
	assembled INTEGER NOT NULL,
	comment TEXT NOT NULL,
	mdf_config_id BLOB,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (mdf_config_id) REFERENCES mdf_door_configs(id)
);

CREATE TRIGGER remove_cabinet_mdf_config AFTER DELETE ON cabinets
BEGIN
	DELETE FROM mdf_door_configs WHERE id = OLD.mdf_config_id;
END;

-- TODO: rename to a more specific `dovetail_drawer_box_configs`
CREATE TABLE drawer_box_configs (
	id BLOB NOT NULL,
	front_material TEXT NOT NULL,
	back_material TEXT NOT NULL,
	side_material TEXT NOT NULL,
	bottom_material TEXT NOT NULL,
	clips TEXT NOT NULL,
	notches TEXT NOT NULL,
	accessory TEXT NOT NULL,
	logo INTEGER NOT NULL,
	post_finish INTEGER NOT NULL,
	scoop_front INTEGER NOT NULL,
	face_mounting_holes INTEGER NOT NULL,
	assembled INTEGER NOT NULL,
	PRIMARY KEY (id)
);

CREATE TABLE doweled_drawer_box_configs (
	id BLOB NOT NULL,
	front_mat_name TEXT NOT NULL,
	front_mat_thickness REAL NOT NULL,
	front_mat_graining INTEGER NOT NULL,
	back_mat_name TEXT NOT NULL,
	back_mat_thickness REAL NOT NULL,
	back_mat_graining INTEGER NOT NULL,
	side_mat_name TEXT NOT NULL,
	side_mat_thickness REAL NOT NULL,
	side_mat_graining INTEGER NOT NULL,
	bottom_mat_name TEXT NOT NULL,
	bottom_mat_thickness REAL NOT NULL,
	bottom_mat_graining INTEGER NOT NULL,
	machine_thickness_for_um INTEGER NOT NULL,
	frontback_height_adjustment REAL NOT NULL,
	PRIMARY KEY (id)
);

CREATE TABLE cabinet_db_configs (
	id BLOB NOT NULL,
	material INTEGER NOT NULL,
	slide_type INTEGER NOT NULL,
	PRIMARY KEY (id)
);

CREATE TABLE mdf_door_configs (
	id BLOB NOT NULL,
	framing_bead TEXT NOT NULL,
	edge_detail TEXT NOT NULL,
	panel_detail TEXT NOT NULL,
	thickness TEXT NOT NULL,
	material TEXT NOT NULL,
	panel_drop REAL,
	paint_color TEXT,
	PRIMARY KEY (id)
);

CREATE TABLE mdf_door_openings (
    id BLOB NOT NULL,
	product_id BLOB NOT NULL,
	opening REAL NOT NULL,
	rail REAL NOT NULL,
	PRIMARY KEY (id)
);

CREATE TABLE base_cabinets (
	product_id BLOB NOT NULL,
	toe_type TEXT NOT NULL,	
	door_qty INTEGER NOT NULL,
	hinge_side INTEGER NOT NULL,
	rollout_positions TEXT NOT NULL,
	rollout_block_type INTEGER NOT NULL,
	rollout_scoop_front INTEGER NOT NULL,
	adj_shelf_qty INTEGER NOT NULL,
	vert_div_qty INTEGER NOT NULL,
	shelf_depth INTEGER NOT NULL,
	drawer_face_height REAL NOT NULL,
	drawer_qty INTEGER NOT NULL,
	db_config_id BLOB,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (db_config_id) REFERENCES cabinet_db_configs(id)
);

CREATE TABLE wall_cabinets (
	product_id BLOB NOT NULL,
	door_qty INTEGER NOT NULL,
	hinge_side INTEGER NOT NULL,
	door_extend_down REAL NOT NULL,
	adj_shelf_qty INTEGER NOT NULL,
	vert_div_qty INTEGER NOT NULL,
	finished_bottom INTEGER NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

CREATE TABLE drawer_base_cabinets (
	product_id BLOB NOT NULL,
	toe_type TEXT NOT NULL,
	face_heights TEXT NOT NULL,
	db_config_id BLOB NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (db_config_id) REFERENCES cabinet_db_configs(id)
);

CREATE TABLE tall_cabinets (
	product_id BLOB NOT NULL,
	toe_type TEXT NOT NULL,
	lower_adj_shelf_qty INTEGER NOT NULL,
	upper_adj_shelf_qty INTEGER NOT NULL,
	lower_vert_div_qty INTEGER NOT NULL,
	upper_vert_div_qty INTEGER NOT NULL,
	rollout_positions TEXT NOT NULL,
	rollout_block_type INTEGER NOT NULL,
	rollout_scoop_front INTEGER NOT NULL,
	lower_door_qty INTEGER NOT NULL,
	upper_door_qty INTEGER NOT NULL,
	lower_door_height REAL NOT NULL,
	hinge_side INTEGER NOT NULL,
	db_config_id BLOB NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (db_config_id) REFERENCES cabinet_db_configs(id)
);

CREATE TABLE sink_cabinets (
	product_id BLOB NOT NULL,
	toe_type TEXT NOT NULL,
	hinge_side INTEGER NOT NULL,
	door_qty INTEGER NOT NULL,
	false_drawer_qty INTEGER NOT NULL,
	drawer_face_height REAL NOT NULL,
	adj_shelf_qty INTEGER NOT NULL,
	shelf_depth INTEGER NOT NULL,
	rollout_positions TEXT NOT NULL,
	rollout_block_type INTEGER NOT NULL,
	rollout_scoop_front INTEGER NOT NULL,
	db_config_id BLOB NOT NULL,
	tilt_front INTEGER NOT NULL,
	scoop_sides INTEGER NOT NULL,
	scoop_depth REAL,
	scoop_from_front REAL,
	scoop_from_back REAL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (db_config_id) REFERENCES cabinet_db_configs(id)
);

CREATE TABLE trash_cabinets (
	product_id BLOB NOT NULL,
	toe_type TEXT NOT NULL,
	trash_config INTEGER NOT NULL,
	drawer_face_height REAL NOT NULL,
	db_config_id BLOB NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (db_config_id) REFERENCES cabinet_db_configs(id)
);

CREATE TABLE diagonal_base_cabinets (
	product_id BLOB NOT NULL,
	toe_type TEXT NOT NULL,
	right_width REAL NOT NULL,
	right_depth REAL NOT NULL,
	hinge_side INTEGER NOT NULL,
	door_qty INTEGER NOT NULL,
	adj_shelf_qty INTEGER NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

CREATE TABLE diagonal_wall_cabinets (
	product_id BLOB NOT NULL,
	right_width REAL NOT NULL,
	right_depth REAL NOT NULL,
	hinge_side INTEGER NOT NULL,
	door_qty INTEGER NOT NULL,
	door_extend_down REAL NOT NULL,
	adj_shelf_qty INTEGER NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

CREATE TABLE pie_cut_base_cabinets (
	product_id BLOB NOT NULL,
	toe_type TEXT NOT NULL,
	right_width REAL NOT NULL,
	right_depth REAL NOT NULL,
	hinge_side INTEGER NOT NULL,
	adj_shelf_qty INTEGER NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

CREATE TABLE pie_cut_wall_cabinets (
	product_id BLOB NOT NULL,
	right_width REAL NOT NULL,
	right_depth REAL NOT NULL,
	hinge_side INTEGER NOT NULL,
	door_extend_down REAL NOT NULL,
	adj_shelf_qty INTEGER NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

CREATE TABLE blind_base_cabinets (
	product_id BLOB NOT NULL,
	toe_type TEXT NOT NULL,
	adj_shelf_qty INTEGER NOT NULL,
	shelf_depth TEXT NOT NULL,
	blind_side INTEGER NOT NULL,
	blind_width REAL NOT NULL,
	door_qty INTEGER NOT NULL,
	hinge_side INTEGER NOT NULL,
	drawer_qty INTEGER NOT NULL,
	drawer_face_height TEXT NOT NULL,
	db_config_id BLOB,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (db_config_id) REFERENCES cabinet_db_configs(id)
);

CREATE TABLE blind_wall_cabinets (
	product_id BLOB NOT NULL,
	blind_side INTEGER NOT NULL,
	blind_width REAL NOT NULL,
	adj_shelf_qty INTEGER NOT NULL,
	door_extend_down REAL NOT NULL,
	door_qty INTEGER NOT NULL,
	hinge_side INTEGER NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);