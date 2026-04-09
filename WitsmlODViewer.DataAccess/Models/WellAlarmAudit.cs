using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WitsmlODViewer.DataAccess.Models;

[Table("well_alarm_audit")]
public class WellAlarmAudit
{
    public int Id { get; set; }

    [Column("well_alarm_id")]
    public int WellAlarmId { get; set; }

    [Column("action")]
    [StringLength(32)]
    public string Action { get; set; } = "";

    [Column("detail")]
    public string? Detail { get; set; }

    [Column("user_id")]
    [StringLength(128)]
    public string? UserId { get; set; }

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }
}
