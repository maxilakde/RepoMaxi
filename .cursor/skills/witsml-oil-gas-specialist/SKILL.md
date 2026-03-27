---
name: witsml-oil-gas-specialist
description: Specialist in oil and gas industry standards WITSML, WITS, and ETP across all versions. Knows all 24+ WITSML objects with validation rules (Read/Write/Update/Delete, parentage, enumerations). Processes large volumes of well data. Use when working with WITSML/ETP, validation, oil and gas data exchange, well logs, mud logs, trajectories, or Energistics standards.
---

# Especialista en Petróleo y Gas: WITSML, WITS y ETP

## Alcance del Agente

Actúa como experto en la industria upstream de petróleo y gas, con conocimiento profundo de:
- **WITSML** (Wellsite Information Transfer Specification Markup Language): estándar para intercambio de datos de pozo
- **WITS** (Wellsite Information Transfer Specification): protocolo histórico y contexto del ecosistema
- **ETP** (Energistics Transfer Protocol): protocolo moderno para transferencia eficiente de datos en tiempo cuasi-real

## Versiones del Estándar

| Versión | Uso |
|---------|-----|
| WITSML 1.4.1.1 | Ampliamente desplegado; objetos como MudLog, Log, Trajectory con estructura XML clásica |
| WITSML 2.0 | Rediseño; MudLog se separa en WellboreGeology y MudLogReport; nuevos namespaces y tipos |
| ETP 1.2 | Recomendado para transferencia en tiempo real; compatible con WITSML 1.4.1.1 y 2.0 |

**Regla de versionado**: Siempre identificar la versión (version="1.4.1.1" o schemaVersion="2.0") al leer o generar XML WITSML. Las diferencias entre versiones afectan estructuras, elementos obligatorios y relaciones entre objetos.

## Jerarquía de Objetos WITSML

Relaciones padre-hijo más comunes:

```
Well
├── Wellbore
│   ├── Trajectory
│   │   └── TrajectoryStation
│   ├── MudLog (v1.4) / WellboreGeology + MudLogReport (v2.0)
│   │   ├── GeologyInterval
│   │   │   └── Lithology
│   │   └── GeologyInterval (interpreted lithologies, show evaluations)
│   ├── Log
│   │   └── LogCurveInfo + datos indexados (depth/time)
│   ├── Rig
│   ├── Tubular
│   │   └── TubularComponent
│   ├── WbGeometry
│   │   └── WbGeometrySection
│   ├── BhaRun
│   └── Message
```

## Objetos Principales (referencia rápida)

| Objeto | Propósito | Campos típicos clave |
|--------|-----------|----------------------|
| **Well** | Pozo como entidad topográfica | uid, name, timeZone, statusWell, dTimCreation, dTimLastChange |
| **Wellbore** | Tramo perforado o completación | uid, wellUid, name, isActive |
| **Trajectory** | Desviación del pozo en el espacio | mdMin, mdMax, gridCorUsed, aziVertSect |
| **TrajectoryStation** | Punto de medición en la trayectoria | md, tvd, incl, azi, dispNs, dispEw |
| **MudLog** | Registro de lodo / litología de cuttings | startMd, endMd, mudLogCompany, geologyInterval |
| **GeologyInterval** | Intervalo geológico en el pozo | mdTop, mdBottom, typeLithology |
| **Lithology** | Descripción litológica | codeLith, lithPc, type |
| **Log** | Curvas de registro (LWD/Wireline) | indexType, objectGrowing, logCurveInfo |
| **Rig** | Equipo de perforación | owner, typeRig |
| **Tubular** | Tubería (casing, tubing) | typeTubularAssy |
| **TubularComponent** | Componente individual | sequence, len, idMeasure, odMeasure |
| **WbGeometry** | Geometría del pozo | dTimReport, mdBottom |
| **WbGeometrySection** | Sección de calibre/hueco | typeHoleCasing, mdTop, mdBottom, idSection, odSection |
| **BhaRun** | Ejecución de BHA | tubularRef, dTimStart, dTimStop |
| **Message** | Mensaje/evento en el pozo | dTim, md, typeMessage, messageText |

Para detalles completos de atributos y versiones, ver [reference.md](reference.md).

## Procesamiento de Grandes Volúmenes

1. **Streaming**: Procesar XML por streaming (XmlReader, SAX, stax) en vez de cargar documentos completos en memoria.
2. **Paginación**: Usar `maxReturnNodes` y `returnElements` en queries WITSML para limitar resultados.
3. **objectGrowing**: Objetos que crecen dinámicamente (Log, MudLog, Trajectory) requieren estrategias de actualización incremental; usar `maxDataNodes` o filtros por MD/time.
4. **Batch**: Insertar/actualizar en lotes (ej. 100–1000 registros) y hacer commit transaccional por lote para evitar timeouts.
5. **Índices**: Para bases de datos, indexar por uid, well_uid, wellbore_uid, md y dTim.
6. **ETP**: Para datos en tiempo real, preferir ETP sobre SOAP/XML; ETP usa mensajes binarios y canales de suscripción más eficientes.

## Convenciones de Nomenclatura

- **uid**: Identificador único en el contexto padre; obligatorio.
- **name**: Nombre legible; recomendado para UX.
- **dTim***: Fechas/horas (datetime).
- **md**, **tvd**: Measured depth, true vertical depth; unidades típicas m o ft según `uom`.
- **uom**: Unidad de medida (ej. m, ft, deg, %).

## Namespaces XML

| Versión | Namespace |
|---------|-----------|
| WITSML 1.4.1.1 | `http://www.witsml.org/schemas/1series` |
| WITSML 2.0/2.1 | `http://www.energistics.org/energyml/data/witsmlv2` |

En 1.4.1.1 los elementos plurales son contenedores: `wells`, `wellbores`, `trajectorys`, `mudlogs`, `logs`, `rigs`, `tubulars`, `wbgeometrys`, `bharuns`, `messages`.

## Mapeo v1.4.1.1 → v2.0/v2.1

Cambios principales al convertir entre versiones:

| v1.4.1.1 | v2.0/2.1 |
|----------|----------|
| Atributos `uidWell`, `uidWellbore` | Elementos `well`, `wellbore` con `uid` como atributo |
| `mdMn`, `mdMx` | `mdMn`, `mdMx` (mismos nombres en v2.1) |
| `commonData` (dTimCreation, dTimLastChange) | `commonData` (igual) |
| `commonTime` (en GeologyInterval) | `commonData` |
| `nameWell`, `nameWellbore` como elementos | Referencias via `well@uid`, `wellbore@uid` |

## Mapeo Típico a Base de Datos

Modelo relacional común (snake_case en columnas):

- Tabla `wells`: uid PK, name, time_zone, status_well, d_tim_creation, d_tim_last_change, source_file
- Tablas hijas: `well_uid` FK (wellbore, trajectory, mud_log, log, rig, tubular, wb_geometry, bha_run, message)
- Objetos compuestos: `trajectory_stations`, `geology_intervals`, `lithologies`, `tubular_components`, `wb_geometry_sections`
- Índices: `(well_uid, wellbore_uid)` en tablas hijas; `(md_top, md_bottom)` en geology_intervals

## Queries WITSML Típicos

**GetWell**: `<wells xmlns="..."><well uid=""/></wells>` con `optionsIn` para `returnElements`.

**GetFromStore / AddToStore**: Usar `maxReturnNodes` y `returnElements=id-only` para listados; `returnElements=all` para datos completos.

**Filtros**: Por `wellbore/well/uid`, `trajectory/uid`, rangos de `md`, `dTim`.

## Errores Comunes y Soluciones

| Problema | Solución |
|----------|----------|
| XML no parsea | Verificar namespace; `trajectorys` vs `trajectories` (ambos válidos en distintas versiones) |
| uid faltante | uid es obligatorio; validar antes de persistir |
| Referencias rotas | Verificar que well_uid/wellbore_uid existan antes de insertar hijos |
| Fechas inválidas | Usar formato ISO 8601; ParseDateTime con CultureInfo.InvariantCulture |
| Objetos duplicados | Usar MERGE/UPSERT por uid en wells; INSERT en tablas hijas con validación |

## Cuándo Aplicar Esta Skill

Usar esta skill cuando el usuario:
- Mencione WITSML, WITS, ETP, Energistics
- Trabaje con pozo, wellbore, trayectoria, mud log, litología, log de perforación
- Necesite parsear, validar o generar XML WITSML
- Tenga problemas de rendimiento con grandes volúmenes de datos de pozo
- Consulte la estructura de objetos (Well, Wellbore, MudLog, etc.)
- Convierta entre versiones WITSML (1.4.1.1 ↔ 2.0/2.1)
- Mapee datos WITSML a bases de datos relacionales

## Patrones del Proyecto WitsmlODViewer

Si el contexto es este repositorio:
- **WitsmlRepository**: Persistencia SQL; métodos Save* por tipo de objeto; MERGE para wells, INSERT para hijos
- **WitsmlProcessor**: Procesa XML con XDocument; usa namespace del root; GetVal() para extraer elementos
- **WitsmlConverter**: Convierte 1.4.1.1 → 2.1; namespaces Witsml1411 y Witsml21; maneja commonData, uidWell→well
- **Base de datos**: CreateWitsmlDatabase.sql; tablas en snake_case; source_file y processed_at en cada tabla
- Soporta: wells, wellbores, trajectories, mud_logs, logs, rigs, tubulars, wb_geometrys, bha_runs, messages

## Recursos Adicionales

- Detalle de objetos: [reference.md](reference.md)
- **Catálogo completo y validación**: [objects-and-validation.md](objects-and-validation.md) — todos los objetos WITSML v1.4.1.1 y v2.0, reglas Read/Write/Update/Delete, parentage, enumeraciones, checklist
- Ejemplos y patrones: [examples.md](examples.md)
