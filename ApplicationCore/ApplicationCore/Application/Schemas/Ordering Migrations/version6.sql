-- Create updated tables

CREATE TABLE drawer_base_cabinets_temp (
	product_id BLOB NOT NULL,
	toe_type TEXT NOT NULL,
	face_heights TEXT NOT NULL,
	db_config_id BLOB,
	is_garage INTEGER NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (db_config_id) REFERENCES cabinet_db_configs(id)
);

CREATE TABLE tall_cabinets_temp (
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
	db_config_id BLOB,
	is_garage INTEGER NOT NULL,
	base_notch_height REAL NOT NULL,
	base_notch_depth REAL NOT NULL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (db_config_id) REFERENCES cabinet_db_configs(id)
);

CREATE TABLE sink_cabinets_temp (
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
	db_config_id BLOB,
	tilt_front INTEGER NOT NULL,
	scoop_sides INTEGER NOT NULL,
	scoop_depth REAL,
	scoop_from_front REAL,
	scoop_from_back REAL,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (db_config_id) REFERENCES cabinet_db_configs(id)
);

CREATE TABLE trash_cabinets_temp (
	product_id BLOB NOT NULL,
	toe_type TEXT NOT NULL,
	trash_config INTEGER NOT NULL,
	drawer_face_height REAL NOT NULL,
	db_config_id BLOB,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (db_config_id) REFERENCES cabinet_db_configs(id)
);




-- Copy data to new tables

INSERT INTO drawer_base_cabinets_temp SELECT * FROM drawer_base_cabinets;
DROP drawer_base_cabinets;
ALTER TABLE drawer_base_cabinets_temp RENAME TO drawer_base_cabinets;

CREATE TRIGGER remove_drawer_base_cabinet_db_config AFTER DELETE ON drawer_base_cabinets 
BEGIN
	DELETE FROM cabinet_db_configs WHERE id = OLD.db_config_id;
END;



INSERT INTO tall_cabinets_temp SELECT * FROM tall_cabinets;
DROP tall_cabinets;
ALTER TABLE tall_cabinets_temp RENAME TO tall_cabinets;

CREATE TRIGGER remove_tall_cabinet_db_config AFTER DELETE ON tall_cabinets 
BEGIN
	DELETE FROM cabinet_db_configs WHERE id = OLD.db_config_id;
END;



INSERT INTO sink_cabinets_temp SELECT * FROM sink_cabinets;
DROP sink_cabinets;
ALTER TABLE sink_cabinets_temp RENAME TO sink_cabinets;

CREATE TRIGGER remove_sink_cabinet_db_config AFTER DELETE ON sink_cabinets 
BEGIN
	DELETE FROM cabinet_db_configs WHERE id = OLD.db_config_id;
END;



INSERT INTO trash_cabinets_temp SELECT * FROM trash_cabinets;
DROP trash_cabinets;
ALTER TABLE trash_cabinets_temp RENAME TO trash_cabinets;

CREATE TRIGGER remove_trash_cabinet_db_config AFTER DELETE ON trash_cabinets 
BEGIN
	DELETE FROM cabinet_db_configs WHERE id = OLD.db_config_id;
END;