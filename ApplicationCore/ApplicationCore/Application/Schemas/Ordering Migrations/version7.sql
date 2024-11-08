-- WARNING - This script will delete all slab door settings from all cabinets

CREATE TABLE cabinet_slab_door_materials (
	id BLOB NOT NULL,
	core INTEGER,
	finish TEXT,
	finish_type INTEGER,
	paint TEXT,
	PRIMARY KEY (id)
);

CREATE TABLE cabinets_temp (
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
	edge_banding_finish TEXT NOT NULL,
	left_side_type INTEGER NOT NULL,
	right_side_type INTEGER NOT NULL,
	assembled INTEGER NOT NULL,
	comment TEXT NOT NULL,
	slab_door_material_id BLOB,
	mdf_config_id BLOB,
	PRIMARY KEY (product_id),
	FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
	FOREIGN KEY (mdf_config_id) REFERENCES mdf_door_configs(id),
	FOREIGN KEY (slab_door_material_id) REFERENCES cabinet_slab_door_materials(id)
);

INSERT INTO cabinets_temp
SELECT
	product_id,
	height,
	width,
	depth,
	box_material_core,
	box_material_finish,
	box_material_finish_type,
	finish_material_core,
	finish_material_finish,
	finish_material_finish_type,
	finish_material_paint,
	edge_banding_finish,
	left_side_type,
	right_side_type,
	assembled,
	comment,
	NULL,
	mdf_config_id
FROM cabinets;
DROP TABLE cabinets;
ALTER TABLE cabinets_temp RENAME TO cabinets;

CREATE TRIGGER remove_cabinet_mdf_config AFTER DELETE ON cabinets
BEGIN
	DELETE FROM mdf_door_configs WHERE id = OLD.mdf_config_id;
END;

CREATE TRIGGER remove_cabinet_slab_door_material AFTER DELETE ON cabinets
BEGIN
	DELETE FROM cabinet_slab_door_materials WHERE id = OLD.slab_door_material_id;
END;