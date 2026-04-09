-- Deja un solo rig por well_uid (se conserva el de id mínimo).
-- Ejecutar: sqlcmd -S .\SQLExpress -E -d WitsmlData -i DeduplicateRigsByWell.sql

USE WitsmlData;
GO

;WITH ranked AS (
    SELECT id,
           ROW_NUMBER() OVER (PARTITION BY well_uid ORDER BY id) AS rn
    FROM rigs
    WHERE well_uid IS NOT NULL
)
DELETE FROM rigs
WHERE id IN (SELECT id FROM ranked WHERE rn > 1);

DECLARE @d INT = @@ROWCOUNT;
PRINT CONCAT('Rigs duplicados eliminados: ', @d);

-- Verificación: no debe haber más de un rig por pozo
IF EXISTS (
    SELECT 1 FROM rigs WHERE well_uid IS NOT NULL GROUP BY well_uid HAVING COUNT(*) > 1
)
    RAISERROR ('Aun hay well_uid con mas de un rig.', 16, 1);
ELSE
    PRINT 'OK: como maximo un rig por pozo.';
GO
