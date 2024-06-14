ALTER TABLE drawer_base_cabinets DROP COLUMN base_notch_height;
ALTER TABLE drawer_base_cabinets DROP COLUMN base_notch_depth;

ALTER TABLE closet_parts ADD COLUMN install_cams INTEGER;
UPDATE closet_parts SET install_cams = 0;