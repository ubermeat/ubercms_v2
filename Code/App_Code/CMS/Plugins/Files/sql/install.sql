﻿-- Create tables
CREATE TABLE fi_dir
(
	dirid							INT PRIMARY KEY AUTO_INCREMENT,
	parent_dirid					INT,
	FOREIGN KEY(`parent_dirid`)		REFERENCES `fi_dir`(`dirid`) ON UPDATE CASCADE ON DELETE CASCADE,
	dir_path						VARCHAR(512) UNIQUE,
	dir_name						VARCHAR(256),
	flags							INT NOT NULL DEFAULT 0,
	
	description						TEXT
);
CREATE TABLE fi_file
(
	fileid							INT PRIMARY KEY AUTO_INCREMENT,
	file							VARCHAR(256),
	extension						VARCHAR(256),
	dirid							INT,
	FOREIGN KEY(`dirid`)			REFERENCES `fi_dir`(`dirid`) ON UPDATE CASCADE ON DELETE CASCADE,
	flags							INT NOT NULL DEFAULT 0,
	UNIQUE(file, dirid),

	description						TEXT,
	size							INT,
	datetime_created				TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
	datetime_modified				TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
);
--CREATE TABLE fi_ips
--(
--	ipid							INT PRIMARY KEY AUTO_INCREMENT,
--	ip								VARCHAR(60) NOT NULL
--);
--CREATE TABLE fi_uniquedownloads
--(
--	fileid							INT NOT NULL,
--	FOREIGN KEY(`fileid`)			REFERENCES `fi_file`(`fileid`) ON UPDATE CASCADE ON DELETE CASCADE,
--	ipid							INT NOT NULL,
--	FOREIGN KEY(`ipid`)				REFERENCES `fi_ips`(`ipid`) ON UPDATE CASCADE ON DELETE CASCADE,
--	PRIMARY KEY(fileid, ipid)
--);
CREATE TABLE fi_extensions
(
	extension						VARCHAR(256) PRIMARY KEY,
	title							VARCHAR(64),
	url_icon						TEXT,
	render_classpath				TEXT,
	render_method					TEXT
);
-- Create views
CREATE OR REPLACE VIEW view_fi_file AS
	SELECT f.*, e.title AS title, e.url_icon AS url_icon, e.render_classpath AS render_classpath, e.render_method AS render_method FROM fi_file AS f LEFT OUTER JOIN fi_extensions AS e ON e.extension=f.extension;

CREATE OR REPLACE VIEW view_fi_dir AS
	SELECT d.* FROM fi_dir AS d;
