namespace WitsmlODViewer.Server.DTOs;

public class WellDTO
{
    public string Uid { get; set; } = "";
    public string? Name { get; set; }
    public string? TimeZone { get; set; }
    public string? StatusWell { get; set; }
    public DateTime? DTimCreation { get; set; }
    public DateTime? DTimLastChange { get; set; }
    public string? SourceFile { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public RigInfoDTO? Rig { get; set; }
    /// <summary>Profundidad del pozo (m) - placeholder hasta feed en tiempo real</summary>
    public decimal? HoleDepth { get; set; }
    /// <summary>Profundidad de perforación / bit (m) - placeholder hasta feed en tiempo real</summary>
    public decimal? DrillDepth { get; set; }
    /// <summary>Actividad actual: Drilling, Tripping, Conditioning, etc. - placeholder hasta feed</summary>
    public string? CurrentActivity { get; set; }
}

public class RigInfoDTO
{
    public string? Name { get; set; }
    public string? Owner { get; set; }
    public string? TypeRig { get; set; }
}
