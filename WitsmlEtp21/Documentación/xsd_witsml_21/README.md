# XSD WITSML 2.1 (EnergyML)

Copia de trabajo del paquete **WITSML v2.1** para análisis de brechas y comparación conceptual con el importador **1.4.1**.

## Archivos de entrada

- **`WitsmlAllObjects.xsd`** — agregador de tipos de objeto WITSML 2.1.
- **`WitsmlCommon.xsd`** — tipos comunes compartidos.
- **~27 XSD** por objeto (`Well.xsd`, `Wellbore.xsd`, `Trajectory.xsd`, `Log.xsd`, `MudLogReport.xsd`, etc.).

Los **nombres y la anidación** no coinciden 1:1 con WITSML 1.4.1; sirven como checklist de objetos y bloques recurrentes.

## Dependencias externas

Algunos esquemas importan tipos **EML** (`common/v2.3`, etc.) con rutas relativas al árbol completo de EnergyML. Si un validador XML falla por `schemaLocation`, completar desde el mismo paquete Energistics o validar solo fragmentos/objeto a objeto.

## Ver también

- [`ReferenciasXsdWitsml.md`](../ReferenciasXsdWitsml.md)
- XSD 1.4.1 del importador: [`../xsd_witsml_141/README.md`](../xsd_witsml_141/README.md)
