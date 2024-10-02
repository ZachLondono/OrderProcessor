ALTER TABLE doweled_drawer_box_configs ADD COLUMN um_notches TEXT;

UPDATE doweled_drawer_box_configs SET um_notches = "Standard Notch" WHERE machine_thickness_for_um = 1;
UPDATE doweled_drawer_box_configs SET um_notches = "No Notch" WHERE machine_thickness_for_um = 0;