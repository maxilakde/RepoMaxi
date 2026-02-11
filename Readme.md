# BochazoEtpWitsml

Herramienta standalone para convertir y procesar archivos WITSML.

## Base de datos

Crear la base de datos en SQL Server Express:

```powershell
sqlcmd -S .\SQLExpress -i Database\CreateWitsmlDatabase.sql
```

O ejecutar el script manualmente en SSMS contra `.\SQLExpress`.

## Uso

**Procesar archivo** (guarda en base de datos):
```
BochazoEtpWitsml <archivo.xml>
```

**Convertir WITSML 1.4.1.1 a 2.1**:
```
BochazoEtpWitsml --convert <archivo-o-directorio>
BochazoEtpWitsml -c <archivo-o-directorio>
```

## Ejemplos

```powershell
# Restaurar paquetes y compilar
dotnet restore
dotnet build

# Procesar archivo (guarda en SQL Server)
dotnet run -- archivo_v2.1.xml

# Convertir un archivo
dotnet run -- -c archivo_1.4.xml

# Convertir un directorio completo
dotnet run -- -c C:\ruta\a\archivos\witsml
```

## Configuración

- **Servidor por defecto:** `.\SQLExpress`
- **Base de datos:** `WitsmlData`
- **Connection string personalizada:** variable de entorno `WITSML_CONNECTION_STRING`
