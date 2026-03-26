# Export OneNote Sedona pages to Word documents
# REQUISITO: Abri OneNote manualmente y deja el notebook Sedona abierto antes de ejecutar.
# Ejecuta en PowerShell (win+x, PowerShell) - NO desde Cursor/IDE.
# Parametro opcional: -MaxPages 10 (para probar con pocas paginas)

param([int]$MaxPages = 0)  # 0 = todas

$sedonaPath = "C:\Users\maxim\CABL y Cubits Dropbox\Oil & Gas\Sedona Project\Sedona"
$outputDir = "C:\Users\maxim\CABL y Cubits Dropbox\Oil & Gas\Sedona_Exportado_Word"
$pfWord = 5  # PublishFormat.pfWord

if (!(Test-Path $sedonaPath)) {
    Write-Host "Error: No existe la carpeta Sedona en: $sedonaPath"
    exit 1
}

New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

try {
    $onenote = New-Object -ComObject OneNote.Application
} catch {
    Write-Host "Error: No se pudo iniciar OneNote. Asegurate de tener OneNote de escritorio instalado."
    exit 1
}

# Abrir el notebook Sedona
$notebookId = [ref]""
try {
    $onenote.OpenHierarchy($sedonaPath, "", $notebookId, 0)  # cftNone
} catch {
    Write-Host "Error al abrir notebook: $_"
    exit 1
}

# Obtener jerarquia completa (hasta paginas)
$hierarchy = [ref]""
try {
    $onenote.GetHierarchy($notebookId.Value, 4, $hierarchy)  # hsPages = 4
} catch {
    Write-Host "Error al obtener jerarquia: $_"
    Write-Host "Intentando con jerarquia global..."
    $onenote.GetHierarchy("", 4, $hierarchy)
}

$xml = [xml]$hierarchy.Value
$ns = @{ one = "http://schemas.microsoft.com/office/onenote/2013/onenote" }
if (!$xml.DocumentElement.NamespaceURI) {
    $ns = @{ one = "http://schemas.microsoft.com/office/onenote/12/2004/onenote" }
}

# Recopilar todas las paginas con su nombre y ruta (seccion)
$pages = @()
$nodeList = $xml.SelectNodes("//one:Page", $ns)
if (!$nodeList -or $nodeList.Count -eq 0) {
    $nodeList = $xml.SelectNodes("//*[local-name()='Page']")
}

foreach ($node in $nodeList) {
    $id = $node.ID
    $name = $node.name
    if (!$name) { $name = "Pagina_sin_nombre" }
    # Sanitizar nombre para archivo
    $safeName = $name -replace '[\\/:*?"<>|]', '_'
    $safeName = $safeName.Substring(0, [Math]::Min(80, $safeName.Length))
    $pages += @{ ID = $id; Name = $safeName }
}

if ($MaxPages -gt 0) {
    $pages = $pages | Select-Object -First $MaxPages
    Write-Host "Modo prueba: exportando solo $MaxPages paginas"
}
Write-Host "Encontradas $($pages.Count) paginas. Exportando a Word..."

$exported = 0
$errors = 0

foreach ($p in $pages) {
    $baseName = $p.Name
    $i = 0
    do {
        $fileName = if ($i -eq 0) { $baseName } else { "${baseName}_$i" }
        $outPath = Join-Path $outputDir "$fileName.docx"
        $i++
    } while (Test-Path $outPath)
    
    try {
        # Publish(pageID, targetPath, format). 4to parametro opcional omitido
        $onenote.Publish($p.ID, $outPath, $pfWord)
        $exported++
        Write-Host "  OK: $fileName"
        Start-Sleep -Milliseconds 300  # Pausa para no saturar OneNote
    } catch {
        $errors++
        Write-Host "  ERROR: $($p.Name) - $_"
        # Si falla RPC, OneNote puede haber dejado de responder - reintentar conexion
        if ($_.Exception.HResult -eq 0x800706BA -and $errors -lt 5) {
            Start-Sleep -Seconds 3
        }
    }
}

Write-Host ""
Write-Host "Listo. Exportados: $exported, Errores: $errors"
Write-Host "Documentos en: $outputDir"
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($onenote) | Out-Null
