using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WitsmlODViewer.DataAccess.Models;

[Table("well_alarms")]
public class WellAlarm
{
    public int Id { get; set; }

    [Column("well_uid")]
    [StringLength(100)]
    public string WellUid { get; set; } = null!;

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = "";

    [Column("variable_mnemonic")]
    [StringLength(64)]
    public string VariableMnemonic { get; set; } = "";

    /// <summary>&gt;, &lt;, =</summary>
    [Column("condition_operator")]
    [StringLength(4)]
    public string ConditionOperator { get; set; } = ">";

    [Column("threshold_value", TypeName = "decimal(18,6)")]
    public decimal ThresholdValue { get; set; }

    [Column("unit")]
    [StringLength(32)]
    public string? Unit { get; set; }

    [Column("second_variable_mnemonic")]
    [StringLength(64)]
    public string? SecondVariableMnemonic { get; set; }

    [Column("second_condition_operator")]
    [StringLength(4)]
    public string? SecondConditionOperator { get; set; }

    [Column("second_threshold_value", TypeName = "decimal(18,6)")]
    public decimal? SecondThresholdValue { get; set; }

    /// <summary>1 Critical … 4 Normal</summary>
    [Column("severity")]
    public int Severity { get; set; } = 2;

    [Column("notify_email")]
    public bool NotifyEmail { get; set; }

    [Column("notify_sms")]
    public bool NotifySms { get; set; }

    [Column("extra_phone")]
    [StringLength(32)]
    public string? ExtraPhone { get; set; }

    [Column("is_public")]
    public bool IsPublic { get; set; }

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [Column("owner_user_id")]
    [StringLength(128)]
    public string? OwnerUserId { get; set; }

    [Column("is_triggered")]
    public bool IsTriggered { get; set; }

    [Column("last_condition_satisfied")]
    public bool LastConditionSatisfied { get; set; }

    [Column("times_triggered")]
    public int TimesTriggered { get; set; }

    [Column("last_triggered_at", TypeName = "datetime2")]
    public DateTime? LastTriggeredAt { get; set; }

    [Column("last_triggered_value_display")]
    [StringLength(128)]
    public string? LastTriggeredValueDisplay { get; set; }

    [Column("last_hole_depth_md", TypeName = "decimal(18,4)")]
    public decimal? LastHoleDepthMd { get; set; }

    [Column("ui_snooze_until", TypeName = "datetime2")]
    public DateTime? UiSnoozeUntil { get; set; }

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; }
}
