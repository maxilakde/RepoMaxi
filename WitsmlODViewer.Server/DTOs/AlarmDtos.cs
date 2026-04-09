namespace WitsmlODViewer.Server.DTOs;

public class AlarmDto
{
    public int Id { get; set; }
    public string WellUid { get; set; } = "";
    public string Name { get; set; } = "";
    public string VariableMnemonic { get; set; } = "";
    public string ConditionOperator { get; set; } = ">";
    public decimal ThresholdValue { get; set; }
    public string? Unit { get; set; }
    public string? SecondVariableMnemonic { get; set; }
    public string? SecondConditionOperator { get; set; }
    public decimal? SecondThresholdValue { get; set; }
    public int Severity { get; set; }
    public bool NotifyEmail { get; set; }
    public bool NotifySms { get; set; }
    public string? ExtraPhone { get; set; }
    public bool IsPublic { get; set; }
    public bool IsEnabled { get; set; }
    public string? OwnerUserId { get; set; }
    public bool IsTriggered { get; set; }
    public int TimesTriggered { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public string? LastTriggeredValueDisplay { get; set; }
    public decimal? LastHoleDepthMd { get; set; }
    public DateTime? UiSnoozeUntil { get; set; }
    public bool IsSubscriptionOnly { get; set; }
    public string? ConditionDisplay { get; set; }
    public string? SecondConditionDisplay { get; set; }
    public string? TimeElapsedDisplay { get; set; }
}

public class AlarmUpsertDto
{
    public string Name { get; set; } = "";
    public string VariableMnemonic { get; set; } = "";
    public string ConditionOperator { get; set; } = ">";
    public decimal ThresholdValue { get; set; }
    public string? Unit { get; set; }
    public string? SecondVariableMnemonic { get; set; }
    public string? SecondConditionOperator { get; set; }
    public decimal? SecondThresholdValue { get; set; }
    public int Severity { get; set; } = 2;
    public bool NotifyEmail { get; set; }
    public bool NotifySms { get; set; }
    public string? ExtraPhone { get; set; }
    public bool IsPublic { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class AlarmEventDto
{
    public int Id { get; set; }
    public int WellAlarmId { get; set; }
    public DateTime TriggeredAt { get; set; }
    public decimal? ValuePrimary { get; set; }
    public decimal? ValueSecondary { get; set; }
    public decimal? HoleDepthMd { get; set; }
    public int Severity { get; set; }
}

public class PagedAlarmEventsDto
{
    public List<AlarmEventDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class EvaluateAlarmsResultDto
{
    public int EvaluatedCount { get; set; }
    public List<AlarmDto> NewlyTriggered { get; set; } = new();
}

public class AlarmNotificationDto
{
    public int Id { get; set; }
    public string WellUid { get; set; } = "";
    public string WellName { get; set; } = "";
    public string Name { get; set; } = "";
    public int Severity { get; set; }
    public DateTime TriggeredAt { get; set; }
    public string? LastTriggeredValueDisplay { get; set; }
}
