-- Migración incremental: campos WITSML 1.4.1 en wells y trajectories (oleada 1).
-- Ejecutar contra WitsmlData si la tabla ya existía sin estas columnas.

USE WitsmlData;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'wells')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('wells') AND name = 'num_api')
        ALTER TABLE wells ADD num_api NVARCHAR(100) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('wells') AND name = 'country')
        ALTER TABLE wells ADD country NVARCHAR(100) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('wells') AND name = 'state')
        ALTER TABLE wells ADD state NVARCHAR(100) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('wells') AND name = 'field')
        ALTER TABLE wells ADD field NVARCHAR(255) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('wells') AND name = 'operator')
        ALTER TABLE wells ADD [operator] NVARCHAR(255) NULL;
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'trajectories')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('trajectories') AND name = 'naming_system')
        ALTER TABLE trajectories ADD naming_system NVARCHAR(100) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('trajectories') AND name = 'definitive')
        ALTER TABLE trajectories ADD definitive VARCHAR(20) NULL;
END
GO

PRINT 'AlterWellsTrajectoriesWitsmlFields aplicado.';
GO
