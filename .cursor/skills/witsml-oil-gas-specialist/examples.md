# Ejemplos y Patrones WITSML

## Fragmento XML Well (v1.4.1.1)

```xml
<wells xmlns="http://www.witsml.org/schemas/1series" version="1.4.1.1">
  <well uid="well-001">
    <name>Pozo Ejemplo 1</name>
    <timeZone>-03:00</timeZone>
    <statusWell>active</statusWell>
    <commonData>
      <dTimCreation>2024-01-15T10:00:00Z</dTimCreation>
      <dTimLastChange>2024-02-20T14:30:00Z</dTimLastChange>
    </commonData>
  </well>
</wells>
```

## Fragmento Wellbore con referencias (v1.4.1.1)

```xml
<wellbores xmlns="http://www.witsml.org/schemas/1series" version="1.4.1.1">
  <wellbore uid="wb-001" uidWell="well-001">
    <name>Pozo Principal</name>
    <isActive>true</isActive>
    <commonData>
      <dTimCreation>2024-01-15T10:00:00Z</dTimCreation>
      <dTimLastChange>2024-02-20T14:30:00Z</dTimLastChange>
    </commonData>
  </wellbore>
</wellbores>
```

## MudLog con GeologyInterval y Lithology

```xml
<mudLog uid="ml-001" uidWell="well-001" uidWellbore="wb-001">
  <name>Mud Log Principal</name>
  <objectGrowing>false</objectGrowing>
  <startMd uom="m">1000</startMd>
  <endMd uom="m">2500</endMd>
  <mudLogCompany>Servicios Geológicos SA</mudLogCompany>
  <geologyInterval uid="gi-001">
    <typeLithology>cuttings</typeLithology>
    <mdTop uom="m">1000</mdTop>
    <mdBottom uom="m">1050</mdBottom>
    <lithology uid="lith-001">
      <type>interval</type>
      <codeLith>SS</codeLith>
      <lithPc uom="%">75</lithPc>
      <description>Arena fina</description>
    </lithology>
  </geologyInterval>
</mudLog>
```

## Parseo en C# con XDocument

```csharp
var doc = XDocument.Parse(xmlContent);
var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
var wells = doc.Root?.Elements(ns + "well").ToList() ?? new List<XElement>();

foreach (var well in wells)
{
    var uid = well.Attribute("uid")?.Value ?? well.Element(ns + "uid")?.Value;
    var name = well.Element(ns + "name")?.Value;
    var commonData = well.Element(ns + "commonData");
    var dTimCreation = commonData?.Element(ns + "dTimCreation")?.Value;
}
```

## Extracción de valores con namespace

```csharp
private static string? GetVal(XElement parent, XNamespace ns, string localName)
{
    return parent.Element(ns + localName)?.Value;
}

// Uso: var name = GetVal(well, ns, "name");
// Para atributos: well.Attribute("uid")?.Value
```

## Switch por tipo de objeto (root)

```csharp
var rootName = doc.Root?.Name.LocalName.ToLower();
switch (rootName)
{
    case "wells":         SaveWells(root, ns, repo); break;
    case "wellbores":    SaveWellbores(root, ns, repo); break;
    case "trajectories":
    case "trajectorys":   SaveTrajectories(root, ns, repo); break;
    case "mudlogs":      SaveMudLogs(root, ns, repo); break;
    case "logs":         SaveLogs(root, ns, repo); break;
    case "rigs":         SaveRigs(root, ns, repo); break;
    case "tubulars":     SaveTubulars(root, ns, repo); break;
    case "wbgeometrys":  SaveWbGeometrys(root, ns, repo); break;
    case "bharuns":      SaveBhaRuns(root, ns, repo); break;
    case "messages":     SaveMessages(root, ns, repo); break;
}
```

## Conversión de referencias v1.4 → v2.1

```csharp
// v1.4: atributos uidWell, uidWellbore
// v2.1: elementos well, wellbore con atributo uid

// Leer en v1.4
var uidWell = elem.Attribute("uidWell")?.Value;

// Escribir en v2.1
if (!string.IsNullOrEmpty(uidWell))
{
    var wellRef = new XElement(ns21 + "well", new XAttribute("uid", uidWell));
    convertedElement.Add(wellRef);
}
```

## MERGE para Wells (evitar duplicados)

```sql
MERGE wells AS target
USING (SELECT @uid AS uid) AS source ON target.uid = source.uid
WHEN MATCHED THEN
  UPDATE SET name = @name, time_zone = @timeZone, ..., processed_at = GETDATE()
WHEN NOT MATCHED THEN
  INSERT (uid, name, time_zone, ...) VALUES (@uid, @name, @timeZone, ...);
```

## Streaming XML para archivos grandes

```csharp
using var reader = XmlReader.Create(filePath, new XmlReaderSettings { Async = true });
await reader.MoveToContentAsync();
while (await reader.ReadAsync())
{
    if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "well")
    {
        var well = XNode.ReadFrom(reader) as XElement;
        if (well != null) ProcessWell(well);
    }
}
```
