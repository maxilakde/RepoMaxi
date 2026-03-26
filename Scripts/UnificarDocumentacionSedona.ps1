# Unifica los 203 .docx de Sedona en un solo documento con TOC y navegacion
# Requiere: Add-Type System.IO.Compression.FileSystem

Add-Type -AssemblyName System.IO.Compression.FileSystem

$inputDir = "C:\Users\maxim\CABL y Cubits Dropbox\Oil & Gas\Sedona_Exportado_Word"
$outputPath = "C:\Users\maxim\CABL y Cubits Dropbox\Oil & Gas\Documentacion_Sedona_Completa.docx"
$enc = [System.Text.Encoding]::UTF8

function Get-TextFromDocx {
    param([string]$docxPath)
    try {
        $tempZip = Join-Path $env:TEMP "unify_$(Get-Random).zip"
        $tempExtract = Join-Path $env:TEMP "unify_extract_$(Get-Random)"
        Copy-Item $docxPath $tempZip -Force
        [System.IO.Compression.ZipFile]::ExtractToDirectory($tempZip, $tempExtract)
        $docPath = Join-Path $tempExtract "word\document.xml"
        if (!(Test-Path $docPath)) { return "" }
        [xml]$doc = Get-Content $docPath -Raw -Encoding UTF8
        $ns = New-Object System.Xml.XmlNamespaceManager($doc.NameTable)
        $ns.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main")
        $textNodes = $doc.SelectNodes("//w:t", $ns)
        $text = ($textNodes | ForEach-Object { $_.InnerText }) -join ""
        Remove-Item $tempZip -Force -ErrorAction SilentlyContinue
        Remove-Item $tempExtract -Recurse -Force -ErrorAction SilentlyContinue
        return $text
    } catch { return "" }
}

function Remove-Tags {
    param([string]$text)
    if ([string]::IsNullOrWhiteSpace($text)) { return "" }
    $t = $text
    $t = $t -replace '</?span[^>]*>', ''
    $t = $t -replace '<[^>]+>', ''
    $t = $t -replace 'style\s*=\s*[''"][^''"]*[''"]', ''
    $t = $t -replace 'lang\s*=\s*[''"][^''"]*[''"]', ''
    $t = $t -replace '\s+', ' '
    $t = $t -replace '^\s+|\s+$', ''
    return $t.Trim()
}

function Remove-DuplicateTitle {
    param([string]$text, [string]$title)
    if ([string]::IsNullOrWhiteSpace($text) -or [string]::IsNullOrWhiteSpace($title)) { return $text }
    $t = $text.Trim()
    $titleEscaped = [regex]::Escape($title)
    if ($t -match "^(?i)$titleEscaped\s*$titleEscaped") {
        $t = $t -replace "^(?i)$titleEscaped\s*$titleEscaped\s*", ""
    } elseif ($t -match "^(?i)$titleEscaped\s") {
        $t = $t -replace "^(?i)$titleEscaped\s*", ""
    }
    return $t.Trim()
}

function Get-SafeBookmarkId {
    param([string]$name)
    $id = $name -replace '[^a-zA-Z0-9]', '_'
    $id = $id -replace '_+', '_'
    $id = $id.Trim('_')
    if ([string]::IsNullOrEmpty($id)) { $id = "s" + [Math]::Abs((Get-Random).GetHashCode() % 10000) }
    return $id
}

function Esc-Xml {
    param([string]$s)
    if ([string]::IsNullOrEmpty($s)) { return "" }
    return [System.Security.SecurityElement]::Escape($s)
}

# Main
Write-Host "Leyendo archivos de: $inputDir"
$files = Get-ChildItem $inputDir -Filter *.docx | Sort-Object Name
if ($files.Count -eq 0) {
    Write-Host "No se encontraron archivos .docx"
    exit 1
}

$sections = @()
foreach ($f in $files) {
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($f.Name)
    $rawText = Get-TextFromDocx $f.FullName
    $cleanText = Remove-Tags $rawText
    $cleanText = Remove-DuplicateTitle $cleanText $baseName
    if ([string]::IsNullOrWhiteSpace($cleanText)) { $cleanText = "(No content)" }

    $parts = $baseName -split '\s*-\s*', 2
    $mainSection = if ($parts.Count -gt 1) { $parts[0].Trim() } else { $baseName }
    $subSection = if ($parts.Count -gt 1) { $parts[1].Trim() } else { $null }

    $sections += @{
        FileName = $baseName
        MainSection = $mainSection
        SubSection = $subSection
        Content = $cleanText
        BookmarkId = Get-SafeBookmarkId $baseName
    }
}

Write-Host "Procesadas $($sections.Count) secciones. Generando documento..."

# Agrupar por seccion principal para orden coherente
$grouped = $sections | Group-Object -Property MainSection | Sort-Object Name
$bodyParts = @()
$tocParts = @()
$bookmarkId = 0

# Portada
$bodyParts += @"
<w:p><w:pPr><w:jc w:val="center"/><w:rPr><w:sz w:val="44"/><w:b/></w:rPr></w:pPr><w:r><w:t>Sedona Project - Consolidated Documentation</w:t></w:r></w:p>
<w:p/><w:p/>
<w:p><w:pPr><w:jc w:val="center"/></w:pPr><w:r><w:t>Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm')</w:t></w:r></w:p>
<w:p/><w:p/><w:p/>
"@

# TOC titulo
$bodyParts += @"
<w:p><w:pPr><w:pStyle w:val="TOCHeading"/></w:pPr><w:r><w:rPr><w:b/><w:sz w:val="28"/></w:rPr><w:t>Table of Contents</w:t></w:r></w:p>
<w:p/>
"@

# TOC entries
foreach ($g in $grouped) {
    foreach ($s in ($g.Group | Sort-Object { $_.SubSection })) {
        $linkText = if ($s.SubSection) { "$($s.MainSection) - $($s.SubSection)" } else { $s.MainSection }
        $tocParts += "<w:p><w:hyperlink w:anchor=`"$($s.BookmarkId)`"><w:r><w:rPr><w:sz w:val=`"20`"/></w:rPr><w:t>$(Esc-Xml $linkText)</w:t></w:r></w:hyperlink></w:p>"
    }
}
$bodyParts += ($tocParts -join "`n")
$bodyParts += "<w:p/><w:p/><w:p/>"

# Contenido
foreach ($g in $grouped) {
    foreach ($s in ($g.Group | Sort-Object { $_.SubSection })) {
        $bookmarkId++
        $sectionTitle = if ($s.SubSection) { "$($s.MainSection) - $($s.SubSection)" } else { $s.MainSection }

        $bodyParts += @"
<w:bookmarkStart w:id="$bookmarkId" w:name="$($s.BookmarkId)"/>
<w:p><w:pPr><w:pStyle w:val="Heading2"/><w:rPr><w:b/><w:sz w:val="24"/></w:rPr></w:pPr><w:r><w:t>$(Esc-Xml $sectionTitle)</w:t></w:r></w:p>
<w:bookmarkEnd w:id="$bookmarkId"/>
"@

        $contentParas = $s.Content -split "`n" | Where-Object { $_.Trim() -ne "" }
        foreach ($para in $contentParas) {
            $escaped = Esc-Xml $para.Trim()
            $bodyParts += "<w:p><w:r><w:t>$escaped</w:t></w:r></w:p>"
        }
        $bodyParts += "<w:p/>"
    }
}

$bodyXml = $bodyParts -join "`n"

$documentXml = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
<w:body>
$bodyXml
</w:body>
</w:document>
"@

# Estilos minimos
$stylesXml = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:styles xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
<w:docDefaults><w:rPrDefault><w:rPr><w:sz w:val="22"/><w:szCs w:val="22"/></w:rPr></w:rPrDefault></w:docDefaults>
<w:style w:type="paragraph" w:styleId="Normal" w:default="1"><w:name w:val="Normal"/><w:qFormat/><w:rPr><w:sz w:val="22"/></w:rPr></w:style>
<w:style w:type="paragraph" w:styleId="Heading1"><w:name w:val="Heading 1"/><w:basedOn w:val="Normal"/><w:rPr><w:b/><w:sz w:val="28"/></w:rPr></w:style>
<w:style w:type="paragraph" w:styleId="Heading2"><w:name w:val="Heading 2"/><w:basedOn w:val="Normal"/><w:rPr><w:b/><w:sz w:val="24"/></w:rPr></w:style>
<w:style w:type="paragraph" w:styleId="TOCHeading"><w:name w:val="TOC Heading"/><w:basedOn w:val="Normal"/><w:rPr><w:b/><w:sz w:val="28"/></w:rPr></w:style>
</w:styles>
"@

$ctXml = @"
<?xml version="1.0" encoding="UTF-8"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
<Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
<Default Extension="xml" ContentType="application/xml"/>
<Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
<Override PartName="/word/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"/>
<Override PartName="/docProps/core.xml" ContentType="application/vnd.openxmlformats-package.core-properties+xml"/>
</Types>
"@

$relsXml = @"
<?xml version="1.0" encoding="UTF-8"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
<Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
<Relationship Id="rId2" Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="docProps/core.xml"/>
</Relationships>
"@

$wordRelsXml = @"
<?xml version="1.0" encoding="UTF-8"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
<Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/document" Target="../document.xml"/>
<Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="../styles.xml"/>
</Relationships>
"@

$coreXml = @"
<?xml version="1.0" encoding="UTF-8"?>
<cp:coreProperties xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties">
<dc:title xmlns:dc="http://purl.org/dc/elements/1.1/">Sedona Project - Consolidated Documentation</dc:title>
<dc:creator xmlns:dc="http://purl.org/dc/elements/1.1/">UnificarDocumentacionSedona</dc:creator>
<dcterms:created xmlns:dcterms="http://purl.org/dc/terms/">$(Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")</dcterms:created>
</cp:coreProperties>
"@

$tempDir = Join-Path $env:TEMP "sedona_unified_$(Get-Random)"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
New-Item -ItemType Directory -Path "$tempDir\_rels" -Force | Out-Null
New-Item -ItemType Directory -Path "$tempDir\word\_rels" -Force | Out-Null
New-Item -ItemType Directory -Path "$tempDir\docProps" -Force | Out-Null

[IO.File]::WriteAllText("$tempDir\[Content_Types].xml", $ctXml, $enc)
[IO.File]::WriteAllText("$tempDir\_rels\.rels", $relsXml, $enc)
[IO.File]::WriteAllText("$tempDir\word\document.xml", $documentXml, $enc)
[IO.File]::WriteAllText("$tempDir\word\styles.xml", $stylesXml, $enc)
[IO.File]::WriteAllText("$tempDir\word\_rels\document.xml.rels", $wordRelsXml, $enc)
[IO.File]::WriteAllText("$tempDir\docProps\core.xml", $coreXml, $enc)

if (Test-Path $outputPath) { Remove-Item $outputPath -Force }
[System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $outputPath)
Remove-Item $tempDir -Recurse -Force

Write-Host "Documento generado: $outputPath"
