-- Corrige textos NVARCHAR insertados con codificación incorrecta.
-- Motivo: sqlcmd sin -f 65001 interpreta el .sql como ANSI y rompe UTF-8 en literales N'...'.
-- Ejecutar: sqlcmd -S .\SQLExpress -E -d WitsmlData -f 65001 -i FixAlarmTextEncoding.sql
USE WitsmlData;
GO

UPDATE well_alarms SET name = N'[Demo] Inclinación y MD (AND)'
WHERE variable_mnemonic = 'incl' AND second_variable_mnemonic = 'md' AND owner_user_id = 'default';

UPDATE well_alarms SET name = N'[Demo] Alarma pública severidad crítica'
WHERE variable_mnemonic = 'dls' AND is_public = 1 AND owner_user_id = 'default';

UPDATE well_alarms SET name = N'[Demo] Ejemplo con último disparo'
WHERE variable_mnemonic = 'tvd' AND times_triggered = 2 AND owner_user_id = 'default';

PRINT 'Textos de alarmas demo corregidos (acentos Unicode).';
GO
