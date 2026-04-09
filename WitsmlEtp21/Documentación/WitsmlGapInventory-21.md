# Inventario: objetos WITSML vs código y base de datos

**Versión importador:** WITSML **1.4.1** (`http://www.witsml.org/schemas/1series`).  
**XSD de referencia cruzada:** WITSML **2.1** (EnergyML) — nombres de elementos difieren; la lista 2.1 indica **cobertura de objetos** a nivel producto.  
**XSD 2.1 en este bootstrap:** [`xsd_witsml_21/`](xsd_witsml_21/) (`WitsmlAllObjects.xsd`). **XSD 1.4.1** en el repo del visor: [`../../Documentación/xsd_witsml_141/`](../../Documentación/xsd_witsml_141/).

## Objetos en `WitsmlAllObjects.xsd` (2.1) — 28 tipos + común

| Objeto 2.1 (XSD) | Tabla / persistencia (visor 1.4.1) | `Save*` en `Witsml141Processor` |
|------------------|----------------------------------------|------------------------------|
| Well | `wells` | Sí (`SaveWells`) |
| Wellbore | `wellbores` | Sí |
| Trajectory | `trajectories` + `trajectory_stations` | Sí |
| MudLogReport (2.1) ≈ MudLog 1.4 | `mud_logs`, `geology_intervals`, `lithologies` | Sí (`SaveMudLogs`) |
| Log | `logs` | Sí |
| Rig | `rigs` | Sí |
| Tubular | `tubulars`, `tubular_components` | Sí |
| WellboreGeometry | `wb_geometrys`, `wb_geometry_sections` | Sí |
| BhaRun | `bha_runs` | Sí |
| Attachment | `attachments` | Sí |
| FormationMarker (2.1) / formationMarker 1.4 | `formation_markers` | Sí |
| Message | `messages` | Sí |
| TrajectoryStation (standalone) | `trajectory_stations` | Sí (standalone) |
| GeologyInterval / Lithology standalone | `geology_intervals`, `lithologies` | Sí |
| TubularComponent standalone | `tubular_components` | Sí |
| WbGeometrySection standalone | `wb_geometry_sections` | Sí |
| **BhaRun** (ya listado) | — | — |
| **CementJob** | — | **No** |
| **DrillReport** | — | **No** |
| **FluidsReport** | — | **No** |
| **OpsReport** | — | **No** |
| **PWLS** | — | **No** |
| **DepthRegImage** | — | **No** |
| **PPFG** | — | **No** |
| **Risk** | — | **No** |
| **StimJob** | — | **No** |
| **Target** | — | **No** |
| **SurveyProgram** | — | **No** |
| **ToolErrorModel** | — | **No** |
| **WellboreGeology** | — | **No** (solo geologyInterval bajo mudLog en 1.4 típico) |
| **WellboreCompletion** | — | **No** |
| **WellCompletion** | — | **No** |
| **DownholeComponent** | — | **No** |
| **WellCMLedger** | — | **No** |
| **WellboreMarkers** (objeto aparte en 2.1) | parcial vía `formation_markers` | Parcial |

## Resumen

- **Cubierto con tabla + import:** 16 rutas de objeto/fragmento listadas arriba con “Sí”.
- **Sin tabla ni import:** tipos 2.1 de reportes (Drill, Ops, Fluids, etc.), PWLS, PPFG, CementJob, StimJob, Risk, Target, SurveyProgram, ToolErrorModel, completions, CMLedger, DownholeComponent, etc.
- Próximos pasos: ver [WitsmlGapMatrix-ObjetosPrincipales.md](WitsmlGapMatrix-ObjetosPrincipales.md) y oleadas de implementación en [WitsmlImplementacionOleadas.md](WitsmlImplementacionOleadas.md).
