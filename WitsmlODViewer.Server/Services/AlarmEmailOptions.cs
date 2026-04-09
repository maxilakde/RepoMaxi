namespace WitsmlODViewer.Server.Services;

public class AlarmEmailOptions
{
    public const string SectionName = "AlarmNotifications";

    /// <summary>Si está vacío, no se envía correo.</summary>
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public bool SmtpUseSsl { get; set; } = true;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public string? FromAddress { get; set; }
}
