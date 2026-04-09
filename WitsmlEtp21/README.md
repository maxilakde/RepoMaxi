# WITSML 2.1 / ETP — bootstrap de repositorio

Este directorio agrupa el **punto de partida** para el producto **WITSML 2.1** (conversión 1.4→2.1, XSD, documentación de brechas) y la referencia a la librería compartida **`Cabl.Witsml.Common`**.

## Contenido

| Ruta | Descripción |
|------|-------------|
| `src/Witsml21.Converter/` | Conversor 1.4.1.1 → 2.1 (extraído del visor 1.4.1). |
| `Documentación/xsd_witsml_21/` | Esquemas WITSML 2.1 (EnergyML). |
| `Documentación/WitsmlGapInventory-21.md` | Inventario de objetos vs estándar 2.1. |

## Migración a Git corporativo (Azure DevOps)

Subir **este árbol** como repositorio nuevo en la organización de la empresa (no incluir el repo del visor 1.4.1 en el mismo remoto salvo que uséis monorepo). Copiar también el proyecto **`Cabl.Witsml.Common`** en la misma solución o publicarlo como NuGet interno.

## Compilar

Desde `WitsmlEtp21/`:

```bash
dotnet build WitsmlEtp21.sln
```

La referencia a `Cabl.Witsml.Common` es relativa a `../Cabl.Witsml.Common/`.

## Próximos pasos de producto

- Cliente **ETP** y API propia.
- Base de datos lógica **WitsmlData21** (ver `Documentación/DatabaseStrategy.md` en el repo del visor 1.4.1).
