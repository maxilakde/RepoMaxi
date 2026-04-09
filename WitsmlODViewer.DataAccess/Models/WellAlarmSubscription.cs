using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WitsmlODViewer.DataAccess.Models;

[Table("well_alarm_subscriptions")]
public class WellAlarmSubscription
{
    public int Id { get; set; }

    [Column("well_alarm_id")]
    public int WellAlarmId { get; set; }

    [Column("subscriber_key")]
    [StringLength(128)]
    public string SubscriberKey { get; set; } = "";

    [Column("notify_email")]
    public bool NotifyEmail { get; set; } = true;

    [Column("notify_sms")]
    public bool NotifySms { get; set; }

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(WellAlarmId))]
    public WellAlarm? WellAlarm { get; set; }
}
