# Catálogo Completo de Objetos WITSML y Reglas de Validación

## Esquemas de Validación por Operación

| Operación | Esquema | Regla |
|-----------|---------|-------|
| **Read** (GetFromStore XMLout) | generated_read_schemas | Todos los elementos opcionales |
| **Write** (AddToStore XMLin) | generated_write_schemas | uid y atributos de identificador único obligatorios |
| **Update** (UpdateInStore XMLin) | generated_update_schemas | uid + uom obligatorios; resto opcional |
| **Delete** (DeleteFromStore QueryIn) | generated_delete_schemas | uid + parentage-pointers obligatorios |

## Jerarquía y Parentage (v1.4.1.1)

Objetos sin padre: **Well**, **CoordinateReferenceSystem**, **ToolErrorModel**, **ToolErrorTermSet**

Objetos con padre Well: **Wellbore**

Objetos con padre Wellbore (uidWell + uidWellbore obligatorios en ETP):
- BhaRun, CementJob, ConvCore, DepthRegImage, DrillReport, FluidsReport
- FormationMarker, Log, MudLog, OpsReport, Rig, Risk, SidewallCore
- StimJob, SurveyProgram, Target, Trajectory, Tubular, WbGeometry
- Attachment, Message (asociados a wellbore)

En ETP con v1.4.1.1: **uidWell** y **uidWellbore** son obligatorios para construir el URI `eml://witsml14/well(uidWell)/wellbore(uidWellbore)/...`

---

## WITSML v1.4.1.1 - Objetos Completos

### Well

| Elemento | Obligatorio | Tipo | Validación |
|----------|-------------|------|------------|
| uid | Sí (Write/Update/Delete) | string | Único en servidor |
| name | No | string | |
| timeZone | Sí (desde 1.4.0) | string | Formato ±HH:MM respecto UTC |
| statusWell | No | enum | active, abandoned, plugged, proposed, sold, suspended, etc. |
| country, state, county, field | No | string | |
| operator, numLicense, dTimLicense | No | | |
| commonData.dTimCreation, dTimLastChange | No | datetime | ISO 8601 |

**Contenedor XML**: `<wells><well uid="..." .../></wells>`

### Wellbore

| Elemento | Obligatorio | Tipo | Validación |
|----------|-------------|------|------------|
| uid | Sí | string | Único dentro del Well |
| uidWell | Sí (parentage) | string | Debe existir Well con ese uid |
| name | No | string | |
| nameWell | No | string | Referencia alternativa |
| isActive | No | boolean | true/false |
| purposeWellbore | No | enum | appraisal, injection, mineral, observation, producer, research, waste disposal, etc. |
| commonData | No | | dTimCreation, dTimLastChange |

**Contenedor XML**: `<wellbores><wellbore uid="..." uidWell="..." .../></wellbores>`

### Trajectory (objeto growing – random)

| Elemento | Obligatorio | Tipo | Validación |
|----------|-------------|------|------------|
| uid | Sí | string | Único en wellbore |
| uidWell, uidWellbore | Sí (parentage) | string | |
| name | No | string | |
| objectGrowing | No | boolean | true si sigue recibiendo stations |
| mdMn, mdMx | No | measure | uom obligatorio si presente |
| trajectoryStation | No | (array) | Cada una con md, tvd, incl, azi |
| commonData | No | | |

**TrajectoryStation**: uid (opcional), md, tvd, incl, azi, dispNs, dispEw, vertSect, dTimStn, typeTrajStation, corUsed

**Contenedor XML**: `<trajectorys>` (typo estándar) o `<trajectories>` según implementación

### MudLog (objeto growing – random)

| Elemento | Obligatorio | Tipo | Validación |
|----------|-------------|------|------------|
| uid | Sí | string | |
| uidWell, uidWellbore | Sí (parentage) | string | |
| name | No | string | |
| objectGrowing | No | boolean | |
| startMd, endMd | No | measure | uom |
| geologyInterval | No | (array) | mdTop, mdBottom obligatorios en intervalo |
| mudLogCompany, mudLogEngineers | No | string | |

**GeologyInterval**: uid, typeLithology, mdTop, mdBottom, lithology[], commonTime

**Lithology**: uid, type, codeLith, lithPc, description

**Contenedor XML**: `<mudlogs>`

### Log (objeto growing – systematic)

| Elemento | Obligatorio | Tipo | Validación |
|----------|-------------|------|------------|
| uid | Sí | string | |
| uidWell, uidWellbore | Sí (parentage) | string | |
| name | No | string | |
| indexType | No | enum | measured depth, date time, elapsed time |
| direction | No | enum | increasing, decreasing |
| objectGrowing | No | boolean | |
| logCurveInfo | No | (array) | mnemonic, unit, typeLogData |
| logData | No | | data: filas delimitadas por \|, columnas por , |

**LogCurveInfo**: mnemonic, unit, curveDescription, typeLogData, minIndex, maxIndex

**Contenedor XML**: `<logs>`

### Rig

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name, owner, typeRig | No | string |
| dTimStartOp, dTimEndOp | No | datetime |

**Contenedor XML**: `<rigs>`

### Tubular

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |
| typeTubularAssy | No | enum | casing, tubing, liner, etc. |
| tubularComponent | No | (array) | |

**TubularComponent**: uid, typeTubularComp, sequence, len, idMeasure, odMeasure, wtPerLen, etc.

**Contenedor XML**: `<tubulars>`

### WbGeometry (Wellbore Geometry)

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |
| dTimReport | No | datetime |
| mdBottom | No | measure |
| wbGeometrySection | No | (array) | |

**WbGeometrySection**: uid, typeHoleCasing, mdTop, mdBottom, idSection, odSection

**Contenedor XML**: `<wbgeometrys>`

### BhaRun

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |
| tubularRef | No | string | uid del Tubular |
| dTimStart, dTimStop | No | datetime |
| numStringRun | No | int |

**Contenedor XML**: `<bharuns>`

### Message

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |
| dTim | No | datetime |
| md | No | measure |
| typeMessage | No | string |
| messageText | No | string |

**Contenedor XML**: `<messages>`

### Attachment

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |
| object | No | base64Binary | Contenido del adjunto |
| description, fileName | No | string |

**Contenedor XML**: `<attachments>`

### CementJob

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |
| cementJobStage | No | (array) | Etapas de cementación |

**Contenedor XML**: `<cementJobs>`

### ConvCore (deprecado en v2.0)

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |

### CoordinateReferenceSystem

Objeto global (sin parentage). Movido a CTA en v2.0.

### DepthRegImage

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |
| depthRegImageRow | No | (array) | Registro de profundidad en imagen |

### DrillReport

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |
| dTimReport | No | datetime |
| reportContent | No | string |

### FluidsReport

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |
| dTim | No | datetime |
| fluidsReportInterval | No | (array) | |

### FormationMarker

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |
| md, tvd | No | measure |
| formationLithology | No | string |

En v2.0: **WellboreMarker** / WellboreMarkerSet

### ObjectGroup

Agrupación de objetos; contexto dependiente.

### OpsReport

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |
| dTimReport | No | datetime |

### Risk

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |
| category | No | string |

### SidewallCore (deprecado en v2.0)

Similar a ConvCore.

### StimJob

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |
| stimJobStage | No | (array) | |

### SurveyProgram

| Elemento | Obligatorio | Tipo |
|----------|-------------|------|
| uid | Sí | string |
| uidWell, uidWellbore | Sí (parentage) | string |
| name | No | string |

### Target (deprecado en v2.0)

Objeto global para objetivos de perforación.

### ToolErrorModel

Objeto global (sin parentage). uid obligatorio.

### ToolErrorTermSet

Objeto global (sin parentage). uid obligatorio.

---

## WITSML v2.0 - Objetos y Cambios

| v1.4.1.1 | v2.0 | Cambio |
|----------|------|--------|
| MudLog | WellboreGeology | Rediseño; incluye CuttingsGeology, InterpretedGeology, ShowEvaluation |
| — | MudLogReport | Nuevo; informes consolidados |
| WbGeometry | WellboreGeometry | Renombrado |
| FormationMarker | WellboreMarker / WellboreMarkerSet | Renombrado |
| Log | Log | Rediseño; Channel, ChannelSet |
| — | RigUtilization | Nuevo; split de Rig |
| — | CementJobEvaluation | Nuevo |
| — | DownholeComponent, WellCMLedger | Nuevos; completación |
| — | WellCompletion, WellboreCompletion | Nuevos |
| ConvCore, SidewallCore, Target | — | Deprecados |

---

## Reglas de Validación Generales

### uid
- Obligatorio en AddToStore, UpdateInStore, DeleteFromStore
- Debe ser único en el contexto del padre
- Recomendado: UUID para interoperabilidad
- No vacío, sin espacios

### uom (unit of measure)
- Obligatorio en UpdateInStore cuando el elemento tiene uom
- Valores de witsmlUnitDict: m, ft, deg, %, s, min, Pa, psi, bar, degC, degF, etc.

### parentage (uidWell, uidWellbore)
- Obligatorio en DeleteFromStore para objetos con padre
- El Well/Wellbore debe existir previamente
- Orden de inserción: Well → Wellbore → hijos

### Fechas (dTim*)
- Formato ISO 8601: `YYYY-MM-DDThh:mm:ss.sssZ` o con offset
- Null/empty si desconocido

### Medidas (measure)
- Valor numérico + atributo uom
- Ejemplo: `<md uom="m">1500.5</md>`

### Objetos growing (Log, MudLog, Trajectory)
- objectGrowing=true: puede recibir datos adicionales
- Usar GrowingObject protocol en ETP para updates incrementales
- maxDataNodes en queries para limitar volumen

---

## Enumeraciones Estándar (enumValues.xml)

Valores típicos del estándar. Consultar enumValues.xml o typ_catalog.xsd para lista exhaustiva.

### statusWell
`active`, `abandoned`, `plugged`, `proposed`, `sold`, `suspended`, `tight`, `inactive`

### purposeWellbore
`appraisal`, `development`, `exploration`, `injection`, `mineral`, `observation`, `producer`, `research`, `unknown`, `waste disposal`

### indexType (Log)
`measured depth`, `date time`, `elapsed time`

### direction (Log)
`increasing`, `decreasing`

### typeTubularAssy
`casing`, `tubing`, `liner`, `drill pipe`, `completion`, `flow line`, `other`

### typeLithology (GeologyInterval)
`cuttings`, `sidewall`, `core`, `paleontology`, `geochem`

### typeTrajStation
`survey`, `gyro`, `mwd`, `unknown`

### typeHoleCasing (WbGeometrySection)
`hole`, `casing`, `open hole`, `liner`

### typeLogData (LogCurveInfo)
`int`, `long`, `short`, `float`, `double`, `string`, `boolean`, `date time`

---

## Checklist de Validación Pre-Persistencia

- [ ] uid presente y no vacío
- [ ] parentage (uidWell, uidWellbore) presente si el objeto lo requiere
- [ ] Well existe antes de Wellbore; Wellbore existe antes de hijos
- [ ] timeZone en Well con formato ±HH:MM (ej. -03:00)
- [ ] Fechas en ISO 8601 o null
- [ ] Medidas con uom cuando el esquema lo exige
- [ ] Valores enum dentro del conjunto permitido (si se valida estricto)
- [ ] Referencias cruzadas (ej. tubularRef en BhaRun) apuntan a uid existente
