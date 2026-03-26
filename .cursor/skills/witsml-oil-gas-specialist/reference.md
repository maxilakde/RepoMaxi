# Referencia de Objetos WITSML

Para el **catálogo completo** de objetos con reglas de validación, ver [objects-and-validation.md](objects-and-validation.md).

## Unidades (uom)

Los valores medidos suelen llevar atributo `uom` (unit of measure):
- **Profundidad**: m, ft
- **Ángulos**: deg
- **Porcentaje**: %
- **Tiempo**: s, min, h
- **Temperatura**: degC, degF
- **Presión**: Pa, psi, bar

Ejemplo: `<md uom="m">1500.5</md>`

## Well

Representa un pozo como entidad topográfica/geográfica. Contenedor raíz para wellbores.

| Elemento | Tipo | Uso |
|----------|------|-----|
| uid | string | Obligatorio, único |
| name | string | Nombre del pozo |
| timeZone | string | Zona horaria (ej. -03:00) |
| statusWell | string | active, abandoned, plugged, etc. |
| dTimCreation | datetime | Fecha de creación |
| dTimLastChange | datetime | Última modificación |
| country | string | País |
| state | string | Estado/región |
| county | string | Condado |
| field | string | Campo |

## Wellbore

Representa un tramo perforado o completación dentro de un Well.

| Elemento | Tipo | Uso |
|----------|------|-----|
| uid | string | Obligatorio |
| nameWell | string | Referencia al Well |
| name | string | Nombre del wellbore |
| isActive | boolean | true/false |
| purposeWellbore | string | exploration, development, etc. |
| dTimCreation | datetime | |
| dTimLastChange | datetime | |

## Trajectory / TrajectoryStation

Define la trayectoria del pozo (desviación) y sus estaciones de medición.

**Trajectory**:
- mdMin, mdMax: rango de MD
- gridCorUsed: corrección de grilla
- aziVertSect, aziRef: referencias de azimut
- dTimTrajStart, dTimTrajEnd

**TrajectoryStation**:
- uid, md, tvd: profundidad medida, vertical verdadera
- incl, azi: inclinación, azimut
- dispNs, dispEw, vertSect: desplazamientos N/S, E/W, sección vertical
- typeTrajStation: survey, gyro, etc.

## MudLog (v1.4.1.1)

Registro de lodo: litología de cuttings, muestras, evaluaciones en superficie.

| Elemento | Tipo | Uso |
|----------|------|-----|
| uid | string | Obligatorio |
| nameWell, nameWellbore | string | Referencias |
| name | string | |
| mudLogCompany | string | Empresa del mud log |
| mudLogEngineers | string | Ingenieros |
| objectGrowing | boolean | Indica si el objeto sigue creciendo |
| startMd, endMd | measure | Rango de MD del mud log |
| geologyInterval | (array) | Intervalos geológicos |

En **v2.0**: se separa en WellboreGeology (interpretaciones) y MudLogReport (informes consolidados).

## GeologyInterval / Lithology

**GeologyInterval**: intervalo geológico dentro de un MudLog.
- uid, mdTop, mdBottom
- typeLithology: cuttings, sidewall, etc.

**Lithology**: descripción litológica dentro del intervalo.
- uid, type, codeLith (código litológico)
- lithPc: porcentaje
- description: texto libre

## Log

Curvas de registro (LWD, wireline, mud logging). Datos indexados por MD o tiempo.

| Elemento | Tipo | Uso |
|----------|------|-----|
| uid | string | Obligatorio |
| nameWell, nameWellbore | string | |
| indexType | string | measured depth, date time, elapsed time |
| direction | string | increasing, decreasing |
| objectGrowing | boolean | Log en crecimiento (tiempo real) |
| logCurveInfo | (array) | Definición de cada curva |
| logData | (array) | Valores indexados (delimitados) |

## Rig / BhaRun

**Rig**: equipo de perforación asociado al wellbore.
- uid, name, owner, typeRig
- dTimStartOp, dTimEndOp

**BhaRun**: ejecución de un BHA (Bottom Hole Assembly).
- tubularRef: referencia al Tubular usado
- dTimStart, dTimStop
- numStringRun: número de string

## Tubular / TubularComponent

**Tubular**: tubería (casing, tubing, liner).
- typeTubularAssy: casing, tubing, liner, etc.

**TubularComponent**: componente individual de la tubería.
- sequence: orden
- typeTubularComp: drill pipe, casing, etc.
- len, idMeasure, odMeasure
- wtPerLen: peso por longitud

## WbGeometry / WbGeometrySection

Geometría del pozo (calibración, diámetros).

**WbGeometry**:
- dTimReport, mdBottom

**WbGeometrySection**:
- typeHoleCasing: hole, casing
- mdTop, mdBottom
- idSection, odSection: diámetros

## Message

Mensaje o evento asociado al pozo/wellbore.

- dTim, md: momento/profundidad del mensaje
- typeMessage: tipo de mensaje
- messageText: contenido

## LogCurveInfo y logData

Cada curva en un Log se define en **logCurveInfo**:
- mnemonic: identificador de la curva (ej. GR, RT, NPHI)
- unit: unidad de medida
- curveDescription: descripción
- typeLogData: int, long, short, float, double, string, boolean, date time
- minIndex, maxIndex: rango del índice

**logData**: valores en filas. Formato típico:
```xml
<logData>
  <data>1000.5,45.2,12.3|1001.0,44.8,12.5|...</data>
</logData>
```
Columnas separadas por `,`; filas por `|`. Orden = index + curvas según logCurveInfo.

## commonData y commonTime

Elementos estándar de auditoría en muchos objetos:

**commonData** (Well, Wellbore, Trajectory, etc.):
- dTimCreation: fecha de creación
- dTimLastChange: última modificación
- comments: comentarios opcionales

**commonTime** (en GeologyInterval y algunos hijos): contiene dTimCreation, dTimLastChange. En v2.0 se unifica en commonData.

## Nombres de elementos: singular vs plural

| v1.4.1.1 | Nota |
|----------|------|
| trajectorys | Con 's' (typo histórico del estándar) |
| mudlogs | Minúscula |
| wbgeometrys | Wellbore Geometry |

En v2.0/v2.1 los contenedores suelen ser plurales estándar: trajectories, mudLogs (camelCase en v2).

## ETP: Canales y Protocolos

ETP opera con canales y mensajes:

- **Discovery**: descubrimiento de capacidades del servidor
- **Store**: operaciones Get, Put, Delete, Find
- **ChannelStreaming**: streaming de canales en tiempo real
- **GrowingObject**: manejo de objetos en crecimiento (Log, MudLog, Trajectory)
- **Dataspace**: navegación de dataspace/directorio

ETP usa formatos binarios (MessagePack) para eficiencia frente a XML SOAP.

### Consideraciones ETP

- Conexión WebSocket; subprotocolo `etp12.energistics.org`
- Autenticación típica: token en handshake
- Para objetos growing: suscribirse a updates en vez de polling
- Timeout y reconexión: definir estrategia para sesiones largas

## Objetos adicionales (WITSML completo)

Otros objetos del estándar que pueden aparecer en proyectos:

| Objeto | Descripción |
|--------|-------------|
| **Activity** | Actividad de perforación (drilling, completion) |
| **ConvCore** | Núcleos de conversión |
| **CementJob** | Trabajo de cementación |
| **StimJob** | Estimulación (fractura, acidificación) |
| **FluidReport** | Reportes de fluido |
| **DepthRegImage** | Imágenes de registro por profundidad |
| **SurveyProgram** | Programa de medición de desviación |

Para esquemas completos, consultar documentación oficial de Energistics.
