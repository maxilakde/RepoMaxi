-- Base de datos WITSML para .\SQLExpress
-- Ejecutar con: sqlcmd -S .\SQLExpress -i CreateWitsmlDatabase.sql

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WitsmlData')
BEGIN
    CREATE DATABASE WitsmlData;
END
GO

USE WitsmlData;
GO

-- Tabla: wells
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'wells')
CREATE TABLE wells (
    uid VARCHAR(100) PRIMARY KEY,
    name NVARCHAR(255),
    time_zone VARCHAR(50),
    status_well VARCHAR(50),
    d_tim_creation DATETIME2,
    d_tim_last_change DATETIME2,
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: wellbores
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'wellbores')
CREATE TABLE wellbores (
    id INT IDENTITY(1,1) PRIMARY KEY,
    uid VARCHAR(100) NOT NULL,
    well_uid VARCHAR(100),
    name NVARCHAR(255),
    is_active VARCHAR(10),
    d_tim_creation DATETIME2,
    d_tim_last_change DATETIME2,
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: trajectories
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'trajectories')
CREATE TABLE trajectories (
    id INT IDENTITY(1,1) PRIMARY KEY,
    uid VARCHAR(100) NOT NULL,
    well_uid VARCHAR(100),
    wellbore_uid VARCHAR(100),
    name NVARCHAR(255),
    service_company NVARCHAR(255),
    object_growing VARCHAR(10),
    d_tim_traj_start DATETIME2,
    d_tim_traj_end DATETIME2,
    md_min DECIMAL(18,4),
    md_max DECIMAL(18,4),
    grid_cor_used DECIMAL(18,6),
    azi_vert_sect DECIMAL(18,6),
    azi_ref VARCHAR(100),
    d_tim_creation DATETIME2,
    d_tim_last_change DATETIME2,
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: trajectory_stations
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'trajectory_stations')
CREATE TABLE trajectory_stations (
    id INT IDENTITY(1,1) PRIMARY KEY,
    trajectory_uid VARCHAR(100) NOT NULL,
    uid VARCHAR(100),
    d_tim_stn DATETIME2,
    type_traj_station VARCHAR(50),
    md DECIMAL(18,4),
    tvd DECIMAL(18,4),
    incl DECIMAL(18,4),
    azi DECIMAL(18,4),
    disp_ns DECIMAL(18,6),
    disp_ew DECIMAL(18,6),
    vert_sect DECIMAL(18,4),
    dls DECIMAL(18,6),
    rate_turn DECIMAL(18,6),
    rate_build DECIMAL(18,6),
    md_delta DECIMAL(18,4),
    tvd_delta DECIMAL(18,4),
    status_traj_station VARCHAR(50),
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: mud_logs
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'mud_logs')
CREATE TABLE mud_logs (
    id INT IDENTITY(1,1) PRIMARY KEY,
    uid VARCHAR(100) NOT NULL,
    well_uid VARCHAR(100),
    wellbore_uid VARCHAR(100),
    name NVARCHAR(255),
    mud_log_company NVARCHAR(255),
    mud_log_engineers NVARCHAR(255),
    object_growing VARCHAR(10),
    d_tim DATETIME2,
    start_md DECIMAL(18,4),
    end_md DECIMAL(18,4),
    d_tim_creation DATETIME2,
    d_tim_last_change DATETIME2,
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: geology_intervals
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'geology_intervals')
CREATE TABLE geology_intervals (
    id INT IDENTITY(1,1) PRIMARY KEY,
    mud_log_uid VARCHAR(100) NOT NULL,
    uid VARCHAR(100),
    type_lithology VARCHAR(50),
    md_top DECIMAL(18,4),
    md_bottom DECIMAL(18,4),
    d_tim_creation DATETIME2,
    d_tim_last_change DATETIME2,
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: lithologies
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'lithologies')
CREATE TABLE lithologies (
    id INT IDENTITY(1,1) PRIMARY KEY,
    geology_interval_uid VARCHAR(100) NOT NULL,
    uid VARCHAR(100),
    type VARCHAR(50),
    code_lith VARCHAR(50),
    lith_pc DECIMAL(18,4),
    description NVARCHAR(500),
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: logs
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'logs')
CREATE TABLE logs (
    id INT IDENTITY(1,1) PRIMARY KEY,
    uid VARCHAR(100) NOT NULL,
    well_uid VARCHAR(100),
    wellbore_uid VARCHAR(100),
    name NVARCHAR(255),
    index_type VARCHAR(50),
    direction VARCHAR(20),
    object_growing VARCHAR(10),
    d_tim_creation DATETIME2,
    d_tim_last_change DATETIME2,
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: rigs
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'rigs')
CREATE TABLE rigs (
    id INT IDENTITY(1,1) PRIMARY KEY,
    uid VARCHAR(100) NOT NULL,
    well_uid VARCHAR(100),
    wellbore_uid VARCHAR(100),
    name NVARCHAR(255),
    owner NVARCHAR(255),
    type_rig VARCHAR(50),
    d_tim_creation DATETIME2,
    d_tim_last_change DATETIME2,
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: tubulars
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tubulars')
CREATE TABLE tubulars (
    id INT IDENTITY(1,1) PRIMARY KEY,
    uid VARCHAR(100) NOT NULL,
    well_uid VARCHAR(100),
    wellbore_uid VARCHAR(100),
    name NVARCHAR(255),
    type_tubular_assy VARCHAR(50),
    d_tim_creation DATETIME2,
    d_tim_last_change DATETIME2,
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: tubular_components
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tubular_components')
CREATE TABLE tubular_components (
    id INT IDENTITY(1,1) PRIMARY KEY,
    tubular_uid VARCHAR(100) NOT NULL,
    uid VARCHAR(100),
    type_tubular_comp VARCHAR(100),
    sequence INT,
    id_measure DECIMAL(18,6),
    od_measure DECIMAL(18,6),
    len DECIMAL(18,4),
    len_joint_av DECIMAL(18,4),
    num_joint_stand INT,
    wt_per_len DECIMAL(18,4),
    vendor NVARCHAR(255),
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: wb_geometrys
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'wb_geometrys')
CREATE TABLE wb_geometrys (
    id INT IDENTITY(1,1) PRIMARY KEY,
    uid VARCHAR(100) NOT NULL,
    well_uid VARCHAR(100),
    wellbore_uid VARCHAR(100),
    name NVARCHAR(255),
    d_tim_report DATETIME2,
    md_bottom DECIMAL(18,4),
    d_tim_creation DATETIME2,
    d_tim_last_change DATETIME2,
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: wb_geometry_sections
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'wb_geometry_sections')
CREATE TABLE wb_geometry_sections (
    id INT IDENTITY(1,1) PRIMARY KEY,
    wb_geometry_uid VARCHAR(100) NOT NULL,
    uid VARCHAR(100),
    type_hole_casing VARCHAR(50),
    md_top DECIMAL(18,4),
    md_bottom DECIMAL(18,4),
    id_section DECIMAL(18,6),
    od_section DECIMAL(18,6),
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: bha_runs
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'bha_runs')
CREATE TABLE bha_runs (
    id INT IDENTITY(1,1) PRIMARY KEY,
    uid VARCHAR(100) NOT NULL,
    well_uid VARCHAR(100),
    wellbore_uid VARCHAR(100),
    name NVARCHAR(255),
    tubular_ref VARCHAR(100),
    d_tim_start DATETIME2,
    d_tim_stop DATETIME2,
    num_string_run INT,
    d_tim_creation DATETIME2,
    d_tim_last_change DATETIME2,
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: attachments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'attachments')
CREATE TABLE attachments (
    id INT IDENTITY(1,1) PRIMARY KEY,
    uid VARCHAR(100) NOT NULL,
    well_uid VARCHAR(100),
    wellbore_uid VARCHAR(100),
    name NVARCHAR(255),
    file_name NVARCHAR(500),
    description NVARCHAR(500),
    d_tim_creation DATETIME2,
    d_tim_last_change DATETIME2,
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: formation_markers
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'formation_markers')
CREATE TABLE formation_markers (
    id INT IDENTITY(1,1) PRIMARY KEY,
    uid VARCHAR(100) NOT NULL,
    well_uid VARCHAR(100),
    wellbore_uid VARCHAR(100),
    name NVARCHAR(255),
    md DECIMAL(18,4),
    tvd DECIMAL(18,4),
    formation_lithology NVARCHAR(255),
    d_tim_creation DATETIME2,
    d_tim_last_change DATETIME2,
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Tabla: messages
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'messages')
CREATE TABLE messages (
    id INT IDENTITY(1,1) PRIMARY KEY,
    uid VARCHAR(100) NOT NULL,
    well_uid VARCHAR(100),
    wellbore_uid VARCHAR(100),
    name NVARCHAR(255),
    d_tim DATETIME2,
    md DECIMAL(18,4),
    type_message VARCHAR(50),
    message_text NVARCHAR(MAX),
    d_tim_creation DATETIME2,
    d_tim_last_change DATETIME2,
    source_file NVARCHAR(500),
    processed_at DATETIME2 DEFAULT GETDATE()
);

-- Migración: añadir direction a logs si no existe
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'logs')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('logs') AND name = 'direction')
BEGIN
    ALTER TABLE logs ADD direction VARCHAR(20) NULL;
    PRINT 'Columna direction añadida a logs.';
END
GO

PRINT 'Base de datos WitsmlData creada/actualizada correctamente.';
GO
