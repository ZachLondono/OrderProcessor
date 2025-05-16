ALTER TABLE mdf_door_configs ADD COLUMN is_open_panel INTEGER NOT NULL DEFAULT 0;
ALTER TABLE mdf_door_openings ADD COLUMN is_open_panel INTEGER NOT NULL DEFAULT 0;
