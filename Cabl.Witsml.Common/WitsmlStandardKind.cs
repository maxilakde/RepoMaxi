namespace Cabl.Witsml.Common;

/// <summary>Clasificación superficial del estándar según namespace o atributo version del raíz.</summary>
public enum WitsmlStandardKind
{
    Unknown = 0,
    /// <summary>WITSML 1.4.x serie 1 (<c>1series</c>).</summary>
    Witsml141Series = 1,
    /// <summary>WITSML 2.x EnergyML (<c>witsmlv2</c>).</summary>
    EnergyMlWitsmlV2 = 2
}
