# Export OneNote a Word usando GetPageContent (evita Publish/RPC)
# Usa contenido XML y crea .docx manualmente. No requiere Publish.
param([int]$MaxPages = 0)

Add-Type -AssemblyName System.IO.Compression.FileSystem

$sedonaPath = "C:\Users\maxim\CABL y Cubits Dropbox\Oil & Gas\Sedona Project\Sedona"
$outputDir = "C:\Users\maxim\CABL y Cubits Dropbox\Oil & Gas\Sedona_Exportado_Word"
$script:Encoding = [System.Text.Encoding]::UTF8

function Extract-TextFromOneNoteXml {
    param([string]$xmlContent)
    $text = ""
    try {
        $xml = [xml]$xmlContent
        $ns = @{ one = $xml.DocumentElement.NamespaceURI }
        if (!$ns.one) { $ns.one = "http://schemas.microsoft.com/office/onenote/2013/onenote" }
        $textNodes = $xml.SelectNodes("//*[local-name()='T']", $null)
        if (!$textNodes) { $textNodes = $xml.SelectNodes("//one:T", $ns) }
        foreach ($n in $textNodes) {
            if ($n.InnerText) { $text += $n.InnerText.Trim() + " " }
        }
        $text = $text.Trim()
    } catch {}
    return $text
}

function New-SimpleDocx {
    param([string]$filePath, [string]$title, [string]$body)
    $tempDir = Join-Path $env:TEMP "docx_$(Get-Random)"
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    New-Item -ItemType Directory -Path "$tempDir\_rels" -Force | Out-Null
    New-Item -ItemType Directory -Path "$tempDir\word\_rels" -Force | Out-Null

    $safeTitle = [System.Security.SecurityElement]::Escape($title)
    $safeBody = [System.Security.SecurityElement]::Escape($body) -replace "`n", "</w:t></w:r></w:p><w:p><w:r><w:t>"
    $docXml = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
<w:body>
<w:p><w:r><w:t>$safeTitle</w:t></w:r></w:p>
<w:p><w:r><w:t>$safeBody</w:t></w:r></w:p>
</w:body>
</w:document>
"@
    $ct = '<?xml version="1.0" encoding="UTF-8"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/></Types>'
    $rels = '<?xml version="1.0" encoding="UTF-8"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/></Relationships>'
    $wordRels = '<?xml version="1.0" encoding="UTF-8"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/document" Target="../document.xml"/></Relationships>'

    [IO.File]::WriteAllText("$tempDir\[Content_Types].xml", $ct, $script:Encoding)
    [IO.File]::WriteAllText("$tempDir\_rels\.rels", $rels, $script:Encoding)
    [IO.File]::WriteAllText("$tempDir\word\document.xml", $docXml, $script:Encoding)
    [IO.File]::WriteAllText("$tempDir\word\_rels\document.xml.rels", $wordRels, $script:Encoding)

    if (Test-Path $filePath) { Remove-Item $filePath -Force }
    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $filePath)
    Remove-Item $tempDir -Recurse -Force
}

# Main
if (!(Test-Path $sedonaPath)) { Write-Host "Error: No existe $sedonaPath"; exit 1 }
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

try { $onenote = New-Object -ComObject OneNote.Application } catch { Write-Host "Error OneNote: $_"; exit 1 }

$notebookId = [ref]""
try { $onenote.OpenHierarchy($sedonaPath, "", $notebookId, 0) } catch { Write-Host "Error abrir notebook: $_"; exit 1 }

$hierarchy = [ref]""
try { $onenote.GetHierarchy($notebookId.Value, 4, $hierarchy) } catch { $onenote.GetHierarchy("", 4, $hierarchy) }

$xml = [xml]$hierarchy.Value
$pages = @()
$nodes = $xml.SelectNodes("//*[local-name()='Page']")
foreach ($n in $nodes) {
    $name = $n.name; if (!$name) { $name = "Pagina_sin_nombre" }
    $safeName = ($name -replace '[\\/:*?"<>|]', '_').Substring(0, [Math]::Min(80, $name.Length))
    $pages += @{ ID = $n.ID; Name = $safeName }
}

if ($MaxPages -gt 0) { $pages = $pages | Select-Object -First $MaxPages }
Write-Host "Exportando $($pages.Count) paginas usando GetPageContent..."

$exported = 0
$i = 0
foreach ($p in $pages) {
    $i++
    $content = [ref]""
    try {
        $onenote.GetPageContent($p.ID, $content, 0, 2)
        $text = Extract-TextFromOneNoteXml $content.Value
        if ([string]::IsNullOrWhiteSpace($text)) { $text = "(Sin contenido de texto)" }
        $outPath = Join-Path $outputDir "$($p.Name).docx"
        $c = 0; while (Test-Path $outPath) { $c++; $outPath = Join-Path $outputDir "$($p.Name)_$c.docx" }
        New-SimpleDocx -filePath $outPath -title $p.Name -body $text
        $exported++
        Write-Host "  OK ($i/$($pages.Count)): $($p.Name)"
    } catch { Write-Host "  ERROR: $($p.Name) - $_" }
    Start-Sleep -Milliseconds 100
}

Write-Host "`nExportados: $exported en $outputDir"
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($onenote) | Out-Null
