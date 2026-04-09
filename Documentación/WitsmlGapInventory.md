# Inventario: objetos WITSML 1.4.1 vs código y base de datos

**Versión importador:** WITSML **1.4.1** (`http://www.witsml.org/schemas/1series`).  
**Extractor:** `Witsml141Processor` / `Witsml141Repository`.  
**XSD:** [`xsd_witsml_141/`](xsd_witsml_141/) (`obj_*.xsd`).

Un inventario cruzado con el estándar **2.1** se mantiene en [`WitsmlEtp21/Documentación/WitsmlGapInventory-21.md`](../WitsmlEtp21/Documentación/WitsmlGapInventory-21.md).

## Objetos 1.4.1 con persistencia en este proyecto

| Objeto / raíz XML | Tabla(s) | Import |
|-------------------|----------|--------|
| wells | `wells` | Sí |
| wellbores | `wellbores` | Sí |
| trajectories | `trajectories`, `trajectory_stations` | Sí |
| mudLogs | `mud_logs`, `geology_intervals`, `lithologies` | Sí |
| logs | `logs` | Sí |
| rigs | `rigs` | Sí |
| tubulars | `tubulars`, `tubular_components` | Sí |
| wbGeometrys | `wb_geometrys`, `wb_geometry_sections` | Sí |
| bhaRuns | `bha_runs` | Sí |
| attachments | `attachments` | Sí |
| formationMarkers | `formation_markers` | Sí |
| messages | `messages` | Sí |
| trajectoryStations (standalone) | `trajectory_stations` | Sí |
| geologyIntervals / lithologies standalone | `geology_intervals`, `lithologies` | Sí |

## Resumen

- Próximos pasos: [WitsmlGapMatrix-ObjetosPrincipales.md](WitsmlGapMatrix-ObjetosPrincipales.md), [WitsmlImplementacionOleadas.md](WitsmlImplementacionOleadas.md).
