# Referencias a esquemas WITSML (visor 1.4.1)

## XSD en este repositorio

| Versión | Carpeta | Uso |
|--------|---------|-----|
| **WITSML 1.4.1** | [`xsd_witsml_141/`](xsd_witsml_141/) | Mismo estándar que el importador (`1series`); gap literal elemento → código. |

El análisis y los XSD **WITSML 2.1** viven en el bootstrap [`WitsmlEtp21/Documentación/xsd_witsml_21/`](../WitsmlEtp21/Documentación/xsd_witsml_21/) (producto separado).

## Importador

Los XML del proyecto y `Witsml141Processor` usan **1.4.1** (`http://www.witsml.org/schemas/1series`).

## Dataset Volve

Colocar los XML de Volve en una ruta acordada por el equipo y ejecutar:

[`Scripts/Measure-WitsmlXmlElementFrequency.ps1`](../Scripts/Measure-WitsmlXmlElementFrequency.ps1)
