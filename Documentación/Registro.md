# Registro de documentación - WitsmlODViewer

Documento de seguimiento: preguntas del usuario y respuestas del asistente.

---

## Sesión 1

### Solicitud

> Agrega una carpeta que se llame Documentación, ahí dentro creá un archivo de word y andá transcribriendo lo que te pido y tus respuestas finales tipo markdown para que me quede.

### Respuesta

Se creó la carpeta **Documentación** y este archivo **Registro.md** en formato Markdown.

**Formato del documento:**
- Puedes abrirlo en Word (Word soporta archivos .md), VS Code, o cualquier editor de texto
- Cada conversación se transcribirá con:
  - Tu pregunta o solicitud
  - La respuesta o solución aplicada
  - Formato tipo Markdown para títulos, listas, código, etc.

**Contenido que ya quedó registrado en sesiones anteriores:**
- Corrección de compilación (exclusión de ArticulosPrueba)
- Eliminación completa de etp-devkit y ArticulosPrueba del proyecto y Git

---

## Sesión 2

### Solicitud

> dejalo como está, cada vez que te pida algo de este Workspace agregá lo que te pido y tu respuesta final allí

### Respuesta

Queda establecido el procedimiento: ante cada solicitud relacionada con este workspace, se añadirá al archivo **Registro.md** tanto la petición como la respuesta final.

---

## Sesión 3

### Solicitud

> Lee un pdf que tengo en la carpeta: "C:\Users\maxim\OneDrive\CABL\Documentación\Sedona", archivo "Sarras Requirements - Online Data Viewer.pdf". Dicho proyecto es un tanto viejo, pero está bueno para entender que me gustaría hacer ahora. Debería ser un proyecto WEB dentro de la solución actual (tomar como referencia para la arquitectura de la solución web lo que hicimos para el Proyecto GestionResidentes). Por ahora, sólo creemos la solución web con una única página donde se puedan ver todos los Wells cargados con la info recabada de los archivos (lo que está en la base de datos WitsmlData).

### Respuesta

Se creó el proyecto **WitsmlODViewer.Web** (ASP.NET Core Razor Pages) dentro de la solución.

**Nota:** No se pudo acceder al PDF (archivo muy grande) ni al proyecto GestionResidentes (no encontrado en el workspace). Se siguió una arquitectura ASP.NET Core estándar.

**Estructura creada:**

- **WitsmlODViewer.Web/** – Proyecto web
  - `Services/WellsService.cs` – Servicio que lee la tabla `wells` de WitsmlData
  - `Pages/Index.cshtml` – Página única con tabla de wells (UID, Nombre, Zona horaria, Estado, fechas, archivo origen)
  - `appsettings.json` – Connection string a `.\SQLExpress`, base de datos `WitsmlData`
  - `Program.cs` – Registro de `IWellsService` en DI

- **Base de datos:** SQL Server Express, base de datos `WitsmlData`, tabla `wells`

**Para ejecutar:**

```powershell
cd c:\TFS-CABL\WitsmlODViewer
dotnet run --project WitsmlODViewer.Web
```

Abre `https://localhost:5xxx` o `http://localhost:5xxx` (el puerto se indica en la consola).

**Connection string:** configurable en `appsettings.json` o `appsettings.Development.json`. Por defecto: `Server=.\SQLExpress;Database=WitsmlData;Trusted_Connection=True;TrustServerCertificate=True;`

---

## Sesión 4

### Solicitud

> no, la solución web así no me sirve, el proyecto GestionResidentes está en C:\TFS-CABL\GestionResidentes

### Respuesta

Se reestructuró la solución web siguiendo la arquitectura de **GestionResidentes** (C:\TFS-CABL\GestionResidentes):

**Estructura actual:**

1. **WitsmlODViewer.DataAccess** – Capa de datos
   - `Models/Well.cs` – Entidad EF Core mapeada a tabla `wells`
   - `WitsmlDataContext.cs` – DbContext con DbSet\<Well\>
   - Entity Framework Core SqlServer

2. **WitsmlODViewer.Server** – API REST + servidor estático
   - `Controllers/WellsController.cs` – GET `/api/v1/wells`
   - `Services/IWellsService`, `WellsService` – Lógica de negocio
   - `DTOs/WellDTO.cs` – DTO para la API
   - Swagger en desarrollo
   - Sirve la SPA desde `witsml.client/dist` en producción
   - SPA Proxy para desarrollo (Vite en puerto 50802)

3. **witsml.client** – SPA Vue 3 + Vite
   - `src/components/WellsList.vue` – Tabla de wells
   - `src/api/index.js` – Cliente axios para `/api/v1`
   - Bootstrap 5, Vue Router

**Para ejecutar:**

```powershell
cd c:\TFS-CABL\WitsmlODViewer
dotnet run --project WitsmlODViewer.Server
```

Abre la URL que muestre la consola (por ejemplo `https://localhost:7238`). En desarrollo, el SPA se sirve desde el proxy de Vite.

---

## Sesión – Agregar Rig al listado de Wells

### Solicitud

> Agregar la información del RIG a listado de Wells tal como está en el PDF (ahora lo podés acceder en: C:\Users\maxim\CABL y Cubits Dropbox\Oil & Gas\Sedona Project)

### Respuesta

Se agregó la información del Rig al listado de wells en la Página de Bienvenida (Lista de Pozos):

1. **WitsmlODViewer.DataAccess**
   - Nuevo modelo `Rig.cs` mapeado a tabla `rigs`
   - `WitsmlDataContext`: agregado `DbSet<Rig>`

2. **WitsmlODViewer.Server**
   - `WellDTO`: nueva propiedad `Rig` de tipo `RigInfoDTO` (name, owner, typeRig)
   - `WellsService`: incluye el Rig asociado a cada well (por `well_uid`)

3. **witsml.client – WelcomePage.vue**
   - Nueva columna **Rig** en la tabla de pozos
   - Muestra: nombre del rig, (owner), tipo (land/offshore/etc.)

**Archivo de prueba:** `Database\Sample\sample_rigs.xml` – procesar con:
```powershell
dotnet run --project .\WitsmlODViewer.csproj -- "Database\Sample\sample_rigs.xml"
```

---

## Sesión – Listado SARRAS y español

### Solicitud

> sí, por favor, y que esté todo en español también (actualizar listado según PDF SARRAS)

### Respuesta

Se actualizó el listado de pozos en la Página de Bienvenida según las especificaciones del PDF SARRAS y todo en español:

**Columnas (orden según SARRAS pág. 12):**
1. (+/−) Expandir
2. Nombre Rig (con filtro)
3. Nombre Pozo (con filtro)
4. Empresa (rig.owner, con filtro)
5. Prof. Pozo (profundidad del pozo – placeholder)
6. Prof. Perf. (profundidad de perforación – placeholder)
7. Actividad actual (con filtro – placeholder)
8. Último dato recibido
9. Estado (Activo/Inactivo, con filtro)
10. Notificaciones
11. Acciones (Alarmas, Chat, Documentos, Exportar)

**Info expandida (Quick Info):**
- Actividad del Rig últimas 24 h
- Último survey (MD, inclinación, acimut, TVD)
- Accesos rápidos → Abrir Página del Pozo

**Traducciones:** Refrescar→Actualizar cada, well(s)→pozo(s), Well Page→Página del Pozo, tiled→mosaico, Well Tag→etiqueta de pozo, etc.

---

## Sesión – Data Statistics (SARRAS 8.0)

### Solicitud

> ahora lee el punto 8.0 en el pdf, la parte de Data Statistics e implementémoslo

### Respuesta

Implementado el módulo **Estadísticas de datos** según SARRAS sección 8.0:

**API:**
- `GET /api/v1/wells/{uid}/statistics/variables` – variables disponibles (md, tvd, incl, azi, etc.)
- `POST /api/v1/wells/{uid}/statistics/data` – datos con rango (profundidad o tiempo) y variables

**Funcionalidades:**
- **Grid de datos**: columnas = variables, filas = datos. Orden por clic en encabezado.
- **Grid de resumen**: Conteo, Suma, Mín, Máx, Media, Desv.Est. (máx 3 por columna).
- **Selección de variables**: checkbox, orden configurable.
- **Rango**: por profundidad (Min/Max m) o por tiempo.
- **Fuente de datos**: `trajectory_stations` (MD, TVD, Incl, Azi, etc.)

**Archivo de prueba:** `Database\Sample\sample_trajectory.xml`
```powershell
dotnet run --project .\WitsmlODViewer.csproj -- "Database\Sample\sample_trajectory.xml"
```

