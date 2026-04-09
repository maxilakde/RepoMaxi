<#
.SYNOPSIS
  Cuenta la frecuencia de nombres de elemento (tags locales) en XML WITSML bajo una carpeta.
.DESCRIPTION
  Útil para priorizar qué campos faltan en el importador según el dataset (p. ej. Volve).
  No requiere XSD; solo recorre el árbol XML.
.PARAMETER Path
  Carpeta raíz con archivos .xml (recursivo).
.PARAMETER Top
  Muestra solo los N primeros por frecuencia.
.EXAMPLE
  .\Measure-WitsmlXmlElementFrequency.ps1 -Path 'D:\datasets\volve' -Top 40
#>
param(
    [Parameter(Mandatory = $true)][string]$Path,
    [int]$Top = 50
)

Add-Type -AssemblyName System.Xml

$counts = [System.Collections.Generic.Dictionary[string, int]]::new([StringComparer]::OrdinalIgnoreCase)

Get-ChildItem -Path $Path -Filter *.xml -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object {
    try {
        $doc = [xml](Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8)
    } catch {
        return
    }
    $walk = {
        param($n)
        if ($null -eq $n) { return }
        foreach ($c in $n.ChildNodes) {
            if ($c.NodeType -ne [System.Xml.XmlNodeType]::Element) { continue }
            $name = $c.LocalName
            if ($counts.ContainsKey($name)) { $counts[$name]++ } else { $counts[$name] = 1 }
            & $walk $c
        }
    }
    & $walk $doc.DocumentElement
}

$counts.GetEnumerator() |
    Sort-Object { $_.Value } -Descending |
    Select-Object -First $Top |
    ForEach-Object { '{0,8}  {1}' -f $_.Value, $_.Key }
