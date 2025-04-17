ALTER TABLE mdf_door_configs ADD COLUMN finish_type INTEGER NOT NULL DEFAULT 0;
ALTER TABLE mdf_door_configs ADD COLUMN finish_color TEXT;

UPDATE mdf_door_configs
SET
	finish_type = 2, -- 2 = Paint, assume that any existing finish type is paint
	finish_color = paint_color
WHERE paint_color IS NOT NULL;

-- Remove the paint_color column, which is now redundant
ALTER TABLE mdf_door_configs DROP COLUMN paint_color;