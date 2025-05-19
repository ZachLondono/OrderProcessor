CREATE TABLE mdf_open_panels (
	id BLOB NOT NULL,
	back_rabbet INTEGER NOT NULL,
	route_for_gasket INTEGER NOT NULL
);

ALTER TABLE mdf_door_configs ADD COLUMN mdf_open_panel_id BLOB DEFAULT NULL;
ALTER TABLE mdf_door_openings ADD COLUMN mdf_open_panel_id BLOB DEFAULT NULL;

