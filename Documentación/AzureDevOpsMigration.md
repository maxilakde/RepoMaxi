# Migración a Git corporativo (Azure DevOps)

## Objetivo

Publicar este repositorio (**visor WITSML 1.4.1**) y el bootstrap **`WitsmlEtp21/`** (o el repo nuevo unificado) en la **organización Azure DevOps** de la empresa, y dejar de usar un remoto Git personal como **origen canónico**.

## Pasos recomendados

1. Crear **proyecto** (o usar existente) en Azure DevOps.
2. Crear **repositorio** vacío (o dos: `WitsmlODViewer141` y `WitsmlEtp21` si separan productos).
3. Desde el clon local con historial completo:
   - `git remote add azure https://dev.azure.com/{org}/{project}/_git/{repo}`
   - `git push azure --all`
   - `git push azure --tags`
4. Sustituir `origin`:
   - `git remote remove origin` (o renombrar a `personal`)
   - `git remote rename azure origin`
5. Configurar **pipelines** (YAML) en la raíz o carpeta `azure-pipelines.yml`: build `dotnet`, tests, publicación.
6. **Variable groups** o **Azure Key Vault** para `WITSML_CONNECTION_STRING` y secretos de SQL.
7. Políticas de rama: proteger `main`, PR obligatorios si aplica.
8. Documentar la **URL del proyecto** en el README interno del equipo.

## Librería COMMON

Publicar **`Witsml.Common`** como paquete NuGet interno (Azure Artifacts) o incluirla en el mismo repo y referenciarla por ruta relativa hasta estabilizar versiones.

## Bootstrap WitsmlEtp21

El directorio [`WitsmlEtp21/`](../WitsmlEtp21/) puede empujarse como **segundo repositorio** o integrarse en monorepo según gobernanza.
