# Matriz de brecha — Well, Wellbore, Trajectory, Log, MudLog (WITSML 1.4.1)

Leyenda: **persistido** = columna + import; **consola** = solo salida debug en modo sin BD; **no** = no leído; **JSON** = candidato a columna `supplemental` / JSON en oleada futura.

## Well (`well`)

| Elemento / concepto 1.4.1 | Estado | Notas |
|---------------------------|--------|--------|
| uid, name | persistido | |
| timeZone, statusWell | persistido | |
| commonData/dTimCreation, dTimLastChange | persistido | |
| numAPI | **persistido** (oleada 1) | API / licencia |
| country, state, field | **persistido** (oleada 1) | ubicación y yacimiento |
| operator | **persistido** (oleada 1) | operador |
| commonData/itemState, commonData/comments | no | candidato JSON |
| customData | no | JSON |
| wellDatum (list) | no | requiere tabla hija |
| wellLocation | no | CRS / geometría |
| groundElevation | no | |
| waterDepth | no | offshore |

## Wellbore (`wellbore`)

| Elemento 1.4.1 | Estado | Notas |
|----------------|--------|--------|
| uid, name, isActive | persistido | |
| commonData fechas | persistido | |
| well ref | persistido | |
| number, suffixAPI | no | |
| commonData extensiones | no | JSON |

## Trajectory (`trajectory`)

| Elemento 1.4.1 | Estado | Notas |
|----------------|--------|--------|
| name, serviceCompany, objectGrowing | persistido | |
| dTimTrajStart/End, mdMn, mdMx | persistido | |
| gridCorUsed, aziVertSect, aziRef | persistido | |
| commonData | persistido | |
| namingSystem | **persistido** (oleada 1) | |
| definitive | **persistido** (oleada 1) | true/false |
| trajectoryStation/* | persistido (subconjunto) | md,tvd,incl,azi,disp*,vertSect,dls,ratios,deltas,status |
| magneticDeclination, trajectoryStation con subnodos | no | ampliar estaciones |
| parentTrajectory | no | |

## Log (`log`)

| Elemento 1.4.1 | Estado | Notas |
|----------------|--------|--------|
| name, indexType, direction, objectGrowing | persistido | |
| commonData | persistido | |
| logCurveInfo (lista de curvas) | **no** | tabla `log_curves` futura |
| startIndex, endIndex | no | |
| step | no | |

## MudLog (`mudLog`)

| Elemento 1.4.1 | Estado | Notas |
|----------------|--------|--------|
| cabecera básica | persistido | |
| geologyInterval / lithology | persistido | parcial |
| parameter (fluidos) | no | |
| showActivity | no | |

## Mapeo conceptual vs WITSML 2.1

Los tipos 2.1 (`Well.xsd`, `Trajectory.xsd`, etc.) reorganizan EML + `commonData`; la **semántica** de negocio (operador, campo, país) se alinea con los elementos 1.4.1 anteriores, no con paths idénticos en XSD 2.1.

**XSD:** 1.4.1 en el visor [`../../Documentación/xsd_witsml_141/`](../../Documentación/xsd_witsml_141/); 2.1 en este bootstrap [`xsd_witsml_21/`](xsd_witsml_21/).
