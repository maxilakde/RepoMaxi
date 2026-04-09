# Oleadas de implementación (gap WITSML)

## Oleada 1 (implementada en código)

- **wells:** columnas `num_api`, `country`, `state`, `field`, `operator` — elementos 1.4.1 `numAPI`, `country`, `state`, `field`, `operator`.
- **trajectories:** columnas `naming_system`, `definitive` — elementos `namingSystem`, `definitive`.
- **API:** `WellDTO` expone los nuevos campos de pozo para el listado en la Página de bienvenida.

**Migración SQL:** bloque al final de [`CreateWitsmlDatabase.sql`](../Database/CreateWitsmlDatabase.sql) y script dedicado [`AlterWellsTrajectoriesWitsmlFields.sql`](../Database/AlterWellsTrajectoriesWitsmlFields.sql). La columna de operador se llama `[operator]` en T-SQL (palabra reservada); en C#/EF sigue mapeada como `Operator` / `operator` en XML WITSML.

## Oleada 2 (sugerida)

- `wellbores`: `number`, `suffixAPI` o referencias API.
- `trajectory_stations`: `magneticDeclination`, `gravAccMagnetic`, `dipAngleUnc` si aparecen en Volve.
- `logs`: tabla hija `log_curves` (mnemonic, unit, typeLogData) + índices start/end.

## Oleada 3 (sugerida)

- Columna `supplemental_xml` o `custom_data_json` NVARCHAR(MAX) en objetos raíz para round-trip sin modelar todo el XSD.
- Import de objetos 2.1: repositorio [`WitsmlEtp21/`](../WitsmlEtp21/) y estrategia 1.4 ↔ 2.x allí.
