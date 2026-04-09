using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WitsmlODViewer.DataAccess.Models;

[Table("well_alarm_events")]
public class WellAlarmEvent
{
    public int Id { get; set; }

    [Column("well_alarm_id")]
    public int WellAlarmId { get; set; }

    [Column("triggered_at", TypeName = "datetime2")]
    public DateTime TriggeredAt { get; set; }

    [Column("value_primary", TypeName = "decimal(18,6)")]
    public decimal? ValuePrimary { get; set; }

    [Column("value_secondary", TypeName = "decimal(18,6)")]
    public decimal? ValueSecondary { get; set; }

    [Column("hole_depth_md", TypeName = "decimal(18,4)")]
    public decimal? HoleDepthMd { get; set; }

    [Column("severity")]
    public int Severity { get; set; }

    [ForeignKey(nameof(WellAlarmId))]
    public WellAlarm? WellAlarm { get; set; }
}
