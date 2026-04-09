using System.Xml.Linq;

namespace Cabl.Witsml.Common;

/// <summary>
/// Detección ligera de versión WITSML (sin validar contra XSD).
/// Usado por herramientas de conteo y por el conversor 1.4→2.1 en el repo ETP21.
/// </summary>
public static class WitsmlXmlVersionDetector
{
    /// <summary>Indica si el contenido XML es WITSML 2.x (namespace o atributo version).</summary>
    public static bool IsWitsml21(string xmlContent)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);
            return IsWitsml21(doc);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Indica si el documento es WITSML 2.x.</summary>
    public static bool IsWitsml21(XDocument doc)
    {
        var root = doc.Root;
        if (root == null) return false;
        var version = root.Attribute("version")?.Value;
        var ns = root.Name.Namespace?.ToString() ?? "";
        return version == "2.0" || version == "2.1" ||
               ns.Contains("energistics.org/energyml/data/witsmlv2", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Versión rápida: primeros bytes del archivo (sin parse completo).</summary>
    public static bool IsWitsml21FromChunk(string xmlChunk)
    {
        if (string.IsNullOrEmpty(xmlChunk)) return false;
        return System.Text.RegularExpressions.Regex.IsMatch(xmlChunk, @"version\s*=\s*[""]2\.[01][""]", System.Text.RegularExpressions.RegexOptions.IgnoreCase) ||
               xmlChunk.Contains("energistics.org/energyml/data/witsmlv2", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Clasifica el documento según namespace/version del raíz.</summary>
    public static WitsmlStandardKind DetectKind(XDocument doc)
    {
        var root = doc.Root;
        if (root == null) return WitsmlStandardKind.Unknown;
        var ns = root.Name.Namespace?.ToString() ?? "";
        if (ns.Contains("energistics.org/energyml/data/witsmlv2", StringComparison.OrdinalIgnoreCase))
            return WitsmlStandardKind.EnergyMlWitsmlV2;
        if (ns.Contains("witsml.org/schemas/1series", StringComparison.OrdinalIgnoreCase) ||
            ns == WitsmlNamespaces.Witsml141Series)
            return WitsmlStandardKind.Witsml141Series;
        var v = root.Attribute("version")?.Value;
        if (v == "2.0" || v == "2.1") return WitsmlStandardKind.EnergyMlWitsmlV2;
        return WitsmlStandardKind.Unknown;
    }
}
