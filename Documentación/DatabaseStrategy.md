# Estrategia de bases de datos (visor 1.4.1 vs producto 2.1/ETP)

## Principio

- **Dos modelos separados:** el visor **WITSML 1.4.1** (`WitsmlODViewer`) usa una base lógica; el futuro producto **2.1/ETP** usará otra (p. ej. `WitsmlData21`). No compartir tablas al inicio.
- **Mismo servidor Azure SQL** (dos bases distintas) suele ser suficiente para coste y operación.

## Visor 1.4.1 (este repositorio)

| Concepto | Valor por defecto local |
|----------|-------------------------|
| Base lógica | `WitsmlData` (histórico); en Azure se recomienda renombrar a **`WitsmlData141`** para claridad. |
| Scripts | [`Database/CreateWitsmlDatabase.sql`](../Database/CreateWitsmlDatabase.sql) |
| Connection string (API) | Clave `WitsmlData` en `appsettings.json`; variable de entorno `WITSML_CONNECTION_STRING` para consola/WinForms. |
| EF Core | `Witsml141DataContext` |

## Producto 2.1/ETP

Definir en el repositorio **`WitsmlEtp21`** (o repo independiente): base **`WitsmlData21`** o `WitsmlEtpData`, migraciones propias, connection string **separada** en Key Vault.

## Azure

- Firewall / Private Endpoint según política.
- **Key Vault** para secretos; no commitear connection strings con contraseña.
- Principio de mínimo privilegio: un usuario SQL o identidad por aplicación.
