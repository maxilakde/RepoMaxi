using WitsmlODViewer.Server.DTOs;

namespace WitsmlODViewer.Server.Services;

public interface IAlarmsService
{
    Task<IReadOnlyList<AlarmDto>> ListAsync(string wellUid, string subscriberKey, CancellationToken ct = default);
    Task<AlarmDto?> GetAsync(string wellUid, int id, string subscriberKey, CancellationToken ct = default);
    Task<AlarmDto> CreateAsync(string wellUid, AlarmUpsertDto dto, string subscriberKey, CancellationToken ct = default);
    Task<AlarmDto?> UpdateAsync(string wellUid, int id, AlarmUpsertDto dto, string subscriberKey, CancellationToken ct = default);
    Task<bool> DeleteAsync(string wellUid, int id, string subscriberKey, CancellationToken ct = default);
    Task<bool> SetEnabledAsync(string wellUid, int id, bool enabled, string subscriberKey, CancellationToken ct = default);
    Task<PagedAlarmEventsDto> GetEventsAsync(string wellUid, int alarmId, int page, int pageSize, string subscriberKey, CancellationToken ct = default);
    Task<EvaluateAlarmsResultDto> EvaluateWellAsync(string wellUid, CancellationToken ct = default);
    Task<IReadOnlyList<AlarmDto>> ListPublicCandidatesAsync(string wellUid, string subscriberKey, CancellationToken ct = default);
    Task<bool> SubscribeAsync(string wellUid, int alarmId, string subscriberKey, bool notifyEmail, bool notifySms, CancellationToken ct = default);
    Task<bool> UnsubscribeAsync(string wellUid, int alarmId, string subscriberKey, CancellationToken ct = default);
    Task<IReadOnlyList<AlarmNotificationDto>> GetPendingUiNotificationsAsync(string wellUid, string subscriberKey, CancellationToken ct = default);
    Task SnoozeAlarmAsync(string wellUid, int id, int minutes, string subscriberKey, CancellationToken ct = default);
    Task DismissUiNotificationAsync(string wellUid, int id, string subscriberKey, CancellationToken ct = default);
    Task<byte[]> ExportEventsToCsvAsync(string wellUid, int alarmId, string subscriberKey, CancellationToken ct = default);
    Task<string> ExportAlarmToXmlAsync(string wellUid, int id, string subscriberKey, CancellationToken ct = default);
    Task<IReadOnlyList<AlarmDto>> ImportAlarmsFromXmlAsync(string wellUid, string xml, string subscriberKey, CancellationToken ct = default);
}
