-- Alarmas de prueba (owner = 'default', coincide con X-Subscriber-Key por defecto en la app)
-- Ajustar @WellUid si querés otro pozo: SELECT uid, name FROM wells;
--
-- IMPORTANTE (acentos / UTF-8): este archivo está en UTF-8. Ejecutar con:
--   sqlcmd -S .\SQLExpress -E -f 65001 -i SampleAlarmSeed.sql
-- Si se omite -f 65001, sqlcmd interpreta el archivo como ANSI y los N'...' con
-- tildes se guardan mal en NVARCHAR. Ver también FixAlarmTextEncoding.sql.

USE WitsmlData;
GO

DECLARE @WellUid VARCHAR(100) = '1bf1cc58-83af-4e13-9696-4fae2f9294ae';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'well_alarms')
BEGIN
    RAISERROR ('Tabla well_alarms no existe. Ejecute CreateWitsmlDatabase.sql primero.', 16, 1);
    RETURN;
END

-- Evitar duplicar si ya se cargó el seed (nombre literal con corchetes)
IF EXISTS (SELECT 1 FROM well_alarms WHERE well_uid = @WellUid AND name = N'[Demo] MD por encima de umbral')
BEGIN
    PRINT 'Seed de alarmas demo ya aplicado para este pozo.';
    RETURN;
END

INSERT INTO well_alarms (
    well_uid, name, variable_mnemonic, condition_operator, threshold_value, unit,
    second_variable_mnemonic, second_condition_operator, second_threshold_value,
    severity, notify_email, notify_sms, extra_phone, is_public, is_enabled, owner_user_id,
    is_triggered, last_condition_satisfied, times_triggered,
    last_triggered_at, last_triggered_value_display, last_hole_depth_md, ui_snooze_until,
    created_at, updated_at
) VALUES
-- 1) Umbral simple (no disparada aún; depende de la última estación de trayectoria al evaluar)
(
    @WellUid,
    N'[Demo] MD por encima de umbral',
    'md', '>', 2500.000000, 'm',
    NULL, NULL, NULL,
    2, 0, 0, NULL, 0, 1, 'default',
    0, 0, 0,
    NULL, NULL, NULL, NULL,
    GETUTCDATE(), GETUTCDATE()
),
-- 2) Dos variables (AND): inclinación y MD
(
    @WellUid,
    N'[Demo] Inclinación y MD (AND)',
    'incl', '>', 30.000000, '°',
    'md', '<', 4000.000000,
    3, 0, 0, NULL, 0, 1, 'default',
    0, 0, 0,
    NULL, NULL, NULL, NULL,
    GETUTCDATE(), GETUTCDATE()
),
-- 3) Alarma pública de demostración (otros usuarios pueden suscribirse)
(
    @WellUid,
    N'[Demo] Alarma pública severidad crítica',
    'dls', '>', 2.000000, '°/30m',
    NULL, NULL, NULL,
    1, 1, 0, NULL, 1, 1, 'default',
    0, 0, 0,
    NULL, NULL, NULL, NULL,
    GETUTCDATE(), GETUTCDATE()
),
-- 4) Con historial simulado (ya disparada, para ver columnas en el grid)
(
    @WellUid,
    N'[Demo] Ejemplo con último disparo',
    'tvd', '<', 5000.000000, 'm',
    NULL, NULL, NULL,
    2, 0, 0, NULL, 0, 1, 'default',
    1, 1, 2,
    DATEADD(HOUR, -2, GETUTCDATE()),
    N'2845.2000',
    2850.5000,
    NULL,
    DATEADD(DAY, -1, GETUTCDATE()), DATEADD(DAY, -1, GETUTCDATE())
);

DECLARE @AlarmId INT = (
    SELECT id FROM well_alarms
    WHERE well_uid = @WellUid AND name = N'[Demo] Ejemplo con último disparo'
);
IF @AlarmId IS NOT NULL
BEGIN
    INSERT INTO well_alarm_events (well_alarm_id, triggered_at, value_primary, value_secondary, hole_depth_md, severity)
    VALUES
        (@AlarmId, DATEADD(DAY, -3, GETUTCDATE()), 2840.000000, NULL, 2845.0000, 2),
        (@AlarmId, DATEADD(HOUR, -2, GETUTCDATE()), 2845.200000, NULL, 2850.5000, 2);
END

PRINT 'Alarmas demo insertadas para well_uid = ' + @WellUid;
GO
