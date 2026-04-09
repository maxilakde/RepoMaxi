-- Un rig ficticio por pozo que aún no tiene fila en rigs (datos demo).
-- Ejecutar: sqlcmd -S .\SQLExpress -E -d WitsmlData -f 65001 -i SeedRigsMissingWells.sql

USE WitsmlData;
GO

;WITH missing AS (
    SELECT w.uid,
           w.name,
           ROW_NUMBER() OVER (ORDER BY w.name, w.uid) AS rn
    FROM wells w
    WHERE NOT EXISTS (SELECT 1 FROM rigs r WHERE r.well_uid = w.uid)
)
INSERT INTO rigs (uid, well_uid, wellbore_uid, name, owner, type_rig, d_tim_creation, d_tim_last_change, processed_at)
SELECT CONCAT(N'RIG-', REPLACE(CAST(NEWID() AS VARCHAR(36)), N'-', N'')),
       m.uid,
       NULL,
       CASE (m.rn % 9)
           WHEN 1 THEN N'Patagonia VII'
           WHEN 2 THEN N'Rig Cuyo Norte'
           WHEN 3 THEN N'Elevador Aconcagua'
           WHEN 4 THEN N'Brava Sur'
           WHEN 5 THEN N'Nordmar III'
           WHEN 6 THEN N'Línea Sur 12'
           WHEN 7 THEN N'Andes Driller 4'
           ELSE N'Pampa Explorer'
       END,
       CASE (m.rn % 7)
           WHEN 1 THEN N'YPF S.A.'
           WHEN 2 THEN N'Petróleos Austral S.A.'
           WHEN 3 THEN N'Tecpetrol S.A.'
           WHEN 4 THEN N'Pluspetrol S.A.'
           WHEN 5 THEN N'Vista Oil & Gas'
           WHEN 6 THEN N'Wintershall DEA'
           ELSE N'CAPEX Operadora'
       END,
       CASE (m.rn % 3)
           WHEN 0 THEN 'Land'
           WHEN 1 THEN 'Onshore'
           ELSE 'Jack-up'
       END,
       GETUTCDATE(),
       GETUTCDATE(),
       GETUTCDATE()
FROM missing m;

DECLARE @n INT = @@ROWCOUNT;
PRINT CONCAT(N'Rigs insertados: ', @n);
GO
