using System.Globalization;
using System.Text;
using System.Xml.Linq;
using WitsmlODViewer.DataAccess;
using WitsmlODViewer.DataAccess.Models;
using WitsmlODViewer.Server.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace WitsmlODViewer.Server.Services;

public class AlarmsService : IAlarmsService
{
    /// <summary>Comparación insensible a mayúsculas para GUID de pozo (URL vs BD).</summary>
    private static string WellKey(string wellUid) => wellUid.Trim().ToLowerInvariant();

    private readonly Witsml141DataContext _db;
    private readonly IAlarmEmailSender _emailSender;
    private readonly AlarmEmailOptions _emailOptions;
    private readonly string? _fallbackRecipient;

    public AlarmsService(
        Witsml141DataContext db,
        IAlarmEmailSender emailSender,
        IOptions<AlarmEmailOptions> emailOptions,
        IConfiguration configuration)
    {
        _db = db;
        _emailSender = emailSender;
        _emailOptions = emailOptions.Value;
        _fallbackRecipient = configuration["AlarmNotifications:FallbackRecipient"];
    }

    public async Task<IReadOnlyList<AlarmDto>> ListAsync(string wellUid, string subscriberKey, CancellationToken ct = default)
    {
        var q = _db.WellAlarms.AsNoTracking().Where(a => a.WellUid.ToLower() == WellKey(wellUid));
        var list = await q.ToListAsync(ct);
        var subIds = await _db.WellAlarmSubscriptions
            .Where(s => s.SubscriberKey == subscriberKey)
            .Select(s => s.WellAlarmId)
            .ToListAsync(ct);
        var subSet = subIds.ToHashSet();

        var filtered = list.Where(a =>
            a.OwnerUserId == subscriberKey
            || (a.IsPublic && subSet.Contains(a.Id))).ToList();

        return filtered.Select(a => MapToDto(a, subscriberKey, subSet)).OrderByDescending(a => a.LastTriggeredAt).ToList();
    }

    public async Task<AlarmDto?> GetAsync(string wellUid, int id, string subscriberKey, CancellationToken ct = default)
    {
        var a = await _db.WellAlarms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.WellUid.ToLower() == WellKey(wellUid), ct);
        if (a == null) return null;
        if (!await CanViewAsync(a, subscriberKey, ct)) return null;
        var subSet = await GetSubscriberSetAsync(subscriberKey, ct);
        return MapToDto(a, subscriberKey, subSet);
    }

    public async Task<AlarmDto> CreateAsync(string wellUid, AlarmUpsertDto dto, string subscriberKey, CancellationToken ct = default)
    {
        NormalizeUpsert(dto);
        var now = DateTime.UtcNow;
        var entity = new WellAlarm
        {
            WellUid = wellUid,
            Name = dto.Name.Trim(),
            VariableMnemonic = dto.VariableMnemonic.Trim(),
            ConditionOperator = dto.ConditionOperator,
            ThresholdValue = dto.ThresholdValue,
            Unit = string.IsNullOrWhiteSpace(dto.Unit) ? null : dto.Unit.Trim(),
            SecondVariableMnemonic = string.IsNullOrWhiteSpace(dto.SecondVariableMnemonic) ? null : dto.SecondVariableMnemonic.Trim(),
            SecondConditionOperator = string.IsNullOrWhiteSpace(dto.SecondConditionOperator) ? null : dto.SecondConditionOperator,
            SecondThresholdValue = dto.SecondThresholdValue,
            Severity = ClampSeverity(dto.Severity),
            NotifyEmail = dto.NotifyEmail,
            NotifySms = dto.NotifySms,
            ExtraPhone = string.IsNullOrWhiteSpace(dto.ExtraPhone) ? null : dto.ExtraPhone.Trim(),
            IsPublic = dto.IsPublic,
            IsEnabled = dto.IsEnabled,
            OwnerUserId = subscriberKey,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.WellAlarms.Add(entity);
        await _db.SaveChangesAsync(ct);
        await AppendAuditAsync(entity.Id, "Create", entity.Name, subscriberKey, ct);
        var subSet = await GetSubscriberSetAsync(subscriberKey, ct);
        return MapToDto(entity, subscriberKey, subSet);
    }

    public async Task<AlarmDto?> UpdateAsync(string wellUid, int id, AlarmUpsertDto dto, string subscriberKey, CancellationToken ct = default)
    {
        var entity = await _db.WellAlarms.FirstOrDefaultAsync(a => a.Id == id && a.WellUid.ToLower() == WellKey(wellUid), ct);
        if (entity == null) return null;
        if (entity.OwnerUserId != subscriberKey) return null;
        NormalizeUpsert(dto);
        entity.Name = dto.Name.Trim();
        entity.VariableMnemonic = dto.VariableMnemonic.Trim();
        entity.ConditionOperator = dto.ConditionOperator;
        entity.ThresholdValue = dto.ThresholdValue;
        entity.Unit = string.IsNullOrWhiteSpace(dto.Unit) ? null : dto.Unit.Trim();
        entity.SecondVariableMnemonic = string.IsNullOrWhiteSpace(dto.SecondVariableMnemonic) ? null : dto.SecondVariableMnemonic.Trim();
        entity.SecondConditionOperator = string.IsNullOrWhiteSpace(dto.SecondConditionOperator) ? null : dto.SecondConditionOperator;
        entity.SecondThresholdValue = dto.SecondThresholdValue;
        entity.Severity = ClampSeverity(dto.Severity);
        entity.NotifyEmail = dto.NotifyEmail;
        entity.NotifySms = dto.NotifySms;
        entity.ExtraPhone = string.IsNullOrWhiteSpace(dto.ExtraPhone) ? null : dto.ExtraPhone.Trim();
        entity.IsPublic = dto.IsPublic;
        entity.IsEnabled = dto.IsEnabled;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await AppendAuditAsync(id, "Update", entity.Name, subscriberKey, ct);
        var subSet = await GetSubscriberSetAsync(subscriberKey, ct);
        return MapToDto(entity, subscriberKey, subSet);
    }

    public async Task<bool> DeleteAsync(string wellUid, int id, string subscriberKey, CancellationToken ct = default)
    {
        var entity = await _db.WellAlarms.FirstOrDefaultAsync(a => a.Id == id && a.WellUid.ToLower() == WellKey(wellUid), ct);
        if (entity == null) return false;
        if (entity.OwnerUserId != subscriberKey)
        {
            var sub = await _db.WellAlarmSubscriptions.FirstOrDefaultAsync(
                s => s.WellAlarmId == id && s.SubscriberKey == subscriberKey, ct);
            if (sub != null)
            {
                _db.WellAlarmSubscriptions.Remove(sub);
                await _db.SaveChangesAsync(ct);
                return true;
            }
            return false;
        }
        await AppendAuditAsync(id, "Delete", entity.Name, subscriberKey, ct);
        _db.WellAlarms.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SetEnabledAsync(string wellUid, int id, bool enabled, string subscriberKey, CancellationToken ct = default)
    {
        var entity = await _db.WellAlarms.FirstOrDefaultAsync(a => a.Id == id && a.WellUid.ToLower() == WellKey(wellUid), ct);
        if (entity == null) return false;
        if (entity.OwnerUserId != subscriberKey) return false;
        entity.IsEnabled = enabled;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await AppendAuditAsync(id, enabled ? "Enable" : "Disable", entity.Name, subscriberKey, ct);
        return true;
    }

    public async Task<PagedAlarmEventsDto> GetEventsAsync(string wellUid, int alarmId, int page, int pageSize, string subscriberKey, CancellationToken ct = default)
    {
        var alarm = await _db.WellAlarms.AsNoTracking().FirstOrDefaultAsync(a => a.Id == alarmId && a.WellUid.ToLower() == WellKey(wellUid), ct);
        if (alarm == null || !await CanViewAsync(alarm, subscriberKey, ct))
            return new PagedAlarmEventsDto { Page = page, PageSize = pageSize };
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 500);
        var q = _db.WellAlarmEvents.AsNoTracking().Where(e => e.WellAlarmId == alarmId).OrderByDescending(e => e.TriggeredAt);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(e => new AlarmEventDto
            {
                Id = e.Id,
                WellAlarmId = e.WellAlarmId,
                TriggeredAt = e.TriggeredAt,
                ValuePrimary = e.ValuePrimary,
                ValueSecondary = e.ValueSecondary,
                HoleDepthMd = e.HoleDepthMd,
                Severity = e.Severity
            }).ToListAsync(ct);
        return new PagedAlarmEventsDto
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<EvaluateAlarmsResultDto> EvaluateWellAsync(string wellUid, CancellationToken ct = default)
    {
        var station = await GetLatestStationAsync(wellUid, ct);
        var alarms = await _db.WellAlarms.Where(a => a.WellUid.ToLower() == WellKey(wellUid) && a.IsEnabled).ToListAsync(ct);
        var result = new EvaluateAlarmsResultDto();
        if (station == null)
        {
            result.EvaluatedCount = alarms.Count;
            return result;
        }

        var newlyTriggered = new List<AlarmDto>();
        foreach (var alarm in alarms)
        {
            var edge = await EvaluateOneAsync(alarm, station, ct);
            if (edge)
            {
                await _db.Entry(alarm).ReloadAsync(ct);
                var subSet = await GetSubscriberSetAsync(alarm.OwnerUserId ?? "", ct);
                newlyTriggered.Add(MapToDto(alarm, alarm.OwnerUserId ?? "", subSet));
            }
        }
        result.EvaluatedCount = alarms.Count;
        result.NewlyTriggered = newlyTriggered;
        return result;
    }

    public async Task<IReadOnlyList<AlarmDto>> ListPublicCandidatesAsync(string wellUid, string subscriberKey, CancellationToken ct = default)
    {
        var subIds = await _db.WellAlarmSubscriptions
            .Where(s => s.SubscriberKey == subscriberKey)
            .Select(s => s.WellAlarmId)
            .ToListAsync(ct);
        var subSet = subIds.ToHashSet();
        var list = await _db.WellAlarms.AsNoTracking()
            .Where(a => a.WellUid.ToLower() == WellKey(wellUid) && a.IsPublic && a.OwnerUserId != subscriberKey && !subSet.Contains(a.Id))
            .ToListAsync(ct);
        return list.Select(a => MapToDto(a, subscriberKey, subSet)).ToList();
    }

    public async Task<bool> SubscribeAsync(string wellUid, int alarmId, string subscriberKey, bool notifyEmail, bool notifySms, CancellationToken ct = default)
    {
        var a = await _db.WellAlarms.FirstOrDefaultAsync(x => x.Id == alarmId && x.WellUid.ToLower() == WellKey(wellUid), ct);
        if (a == null || !a.IsPublic || a.OwnerUserId == subscriberKey) return false;
        if (await _db.WellAlarmSubscriptions.AnyAsync(s => s.WellAlarmId == alarmId && s.SubscriberKey == subscriberKey, ct))
            return true;
        _db.WellAlarmSubscriptions.Add(new WellAlarmSubscription
        {
            WellAlarmId = alarmId,
            SubscriberKey = subscriberKey,
            NotifyEmail = notifyEmail,
            NotifySms = notifySms,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UnsubscribeAsync(string wellUid, int alarmId, string subscriberKey, CancellationToken ct = default)
    {
        var sub = await _db.WellAlarmSubscriptions.FirstOrDefaultAsync(
            s => s.WellAlarmId == alarmId && s.SubscriberKey == subscriberKey, ct);
        if (sub == null) return false;
        _db.WellAlarmSubscriptions.Remove(sub);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<AlarmNotificationDto>> GetPendingUiNotificationsAsync(string wellUid, string subscriberKey, CancellationToken ct = default)
    {
        var alarms = await _db.WellAlarms.AsNoTracking()
            .Where(a => a.WellUid.ToLower() == WellKey(wellUid) && a.IsTriggered)
            .ToListAsync(ct);
        var well = await _db.Wells.AsNoTracking().FirstOrDefaultAsync(w => w.Uid.ToLower() == WellKey(wellUid), ct);
        var wellName = well?.Name ?? wellUid ?? "";
        var now = DateTime.UtcNow;
        var list = new List<AlarmNotificationDto>();
        foreach (var a in alarms)
        {
            if (!await CanViewAsync(a, subscriberKey, ct)) continue;
            if (a.UiSnoozeUntil.HasValue && a.UiSnoozeUntil.Value > now) continue;
            if (a.LastTriggeredAt == null) continue;
            list.Add(new AlarmNotificationDto
            {
                Id = a.Id,
                WellUid = wellUid ?? "",
                WellName = wellName ?? "",
                Name = a.Name ?? "",
                Severity = a.Severity,
                TriggeredAt = a.LastTriggeredAt.Value,
                LastTriggeredValueDisplay = a.LastTriggeredValueDisplay
            });
        }
        return list;
    }

    public async Task SnoozeAlarmAsync(string wellUid, int id, int minutes, string subscriberKey, CancellationToken ct = default)
    {
        var a = await _db.WellAlarms.FirstOrDefaultAsync(x => x.Id == id && x.WellUid.ToLower() == WellKey(wellUid), ct);
        if (a == null || !await CanViewAsync(a, subscriberKey, ct)) return;
        a.UiSnoozeUntil = DateTime.UtcNow.AddMinutes(Math.Max(0, minutes));
        a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DismissUiNotificationAsync(string wellUid, int id, string subscriberKey, CancellationToken ct = default)
    {
        var a = await _db.WellAlarms.FirstOrDefaultAsync(x => x.Id == id && x.WellUid.ToLower() == WellKey(wellUid), ct);
        if (a == null || !await CanViewAsync(a, subscriberKey, ct)) return;
        a.UiSnoozeUntil = DateTime.UtcNow.AddYears(10);
        a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<byte[]> ExportEventsToCsvAsync(string wellUid, int alarmId, string subscriberKey, CancellationToken ct = default)
    {
        var alarm = await _db.WellAlarms.AsNoTracking().FirstOrDefaultAsync(a => a.Id == alarmId && a.WellUid.ToLower() == WellKey(wellUid), ct);
        if (alarm == null || !await CanViewAsync(alarm, subscriberKey, ct)) return Array.Empty<byte>();
        var events = await _db.WellAlarmEvents.AsNoTracking()
            .Where(e => e.WellAlarmId == alarmId)
            .OrderByDescending(e => e.TriggeredAt)
            .ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("TriggeredAt,Severity,ValuePrimary,ValueSecondary,HoleDepthMd");
        foreach (var e in events)
        {
            sb.AppendLine(string.Join(',',
                EscapeCsv(e.TriggeredAt.ToString("o", CultureInfo.InvariantCulture)),
                e.Severity.ToString(CultureInfo.InvariantCulture),
                e.ValuePrimary?.ToString(CultureInfo.InvariantCulture) ?? "",
                e.ValueSecondary?.ToString(CultureInfo.InvariantCulture) ?? "",
                e.HoleDepthMd?.ToString(CultureInfo.InvariantCulture) ?? ""));
        }
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task<string> ExportAlarmToXmlAsync(string wellUid, int id, string subscriberKey, CancellationToken ct = default)
    {
        var a = await _db.WellAlarms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.WellUid.ToLower() == WellKey(wellUid), ct);
        if (a == null || a.OwnerUserId != subscriberKey) return "";
        var root = new XElement("Alarm",
            new XElement("Name", a.Name),
            new XElement("VariableMnemonic", a.VariableMnemonic),
            new XElement("ConditionOperator", a.ConditionOperator),
            new XElement("ThresholdValue", a.ThresholdValue.ToString(CultureInfo.InvariantCulture)),
            new XElement("Unit", a.Unit ?? ""),
            new XElement("SecondVariableMnemonic", a.SecondVariableMnemonic ?? ""),
            new XElement("SecondConditionOperator", a.SecondConditionOperator ?? ""),
            new XElement("SecondThresholdValue", a.SecondThresholdValue?.ToString(CultureInfo.InvariantCulture) ?? ""),
            new XElement("Severity", a.Severity),
            new XElement("NotifyEmail", a.NotifyEmail),
            new XElement("NotifySms", a.NotifySms),
            new XElement("ExtraPhone", a.ExtraPhone ?? ""),
            new XElement("IsPublic", a.IsPublic),
            new XElement("IsEnabled", a.IsEnabled));
        return root.ToString(SaveOptions.None);
    }

    public async Task<IReadOnlyList<AlarmDto>> ImportAlarmsFromXmlAsync(string wellUid, string xml, string subscriberKey, CancellationToken ct = default)
    {
        var doc = XDocument.Parse(xml);
        if (doc.Root == null) return Array.Empty<AlarmDto>();
        IEnumerable<XElement> elements = doc.Root.Name.LocalName == "Alarms"
            ? doc.Root.Elements("Alarm")
            : new[] { doc.Root };
        var created = new List<AlarmDto>();
        foreach (var el in elements)
        {
            if (el == null) continue;
            var dto = new AlarmUpsertDto
            {
                Name = (string?)el.Element("Name") ?? "Imported",
                VariableMnemonic = (string?)el.Element("VariableMnemonic") ?? "md",
                ConditionOperator = (string?)el.Element("ConditionOperator") ?? ">",
                ThresholdValue = decimal.TryParse((string?)el.Element("ThresholdValue"), NumberStyles.Any, CultureInfo.InvariantCulture, out var t) ? t : 0,
                Unit = (string?)el.Element("Unit"),
                SecondVariableMnemonic = string.IsNullOrEmpty((string?)el.Element("SecondVariableMnemonic")) ? null : (string?)el.Element("SecondVariableMnemonic"),
                SecondConditionOperator = string.IsNullOrEmpty((string?)el.Element("SecondConditionOperator")) ? null : (string?)el.Element("SecondConditionOperator"),
                SecondThresholdValue = decimal.TryParse((string?)el.Element("SecondThresholdValue"), NumberStyles.Any, CultureInfo.InvariantCulture, out var t2) ? t2 : null,
                Severity = int.TryParse((string?)el.Element("Severity"), out var s) ? s : 2,
                NotifyEmail = bool.TryParse((string?)el.Element("NotifyEmail"), out var ne) && ne,
                NotifySms = bool.TryParse((string?)el.Element("NotifySms"), out var ns) && ns,
                ExtraPhone = (string?)el.Element("ExtraPhone"),
                IsPublic = bool.TryParse((string?)el.Element("IsPublic"), out var ip) && ip,
                IsEnabled = !bool.TryParse((string?)el.Element("IsEnabled"), out var ie) || ie
            };
            var alarm = await CreateAsync(wellUid, dto, subscriberKey, ct);
            created.Add(alarm);
        }
        return created;
    }

    private async Task<bool> EvaluateOneAsync(WellAlarm alarm, TrajectoryStation station, CancellationToken ct)
    {
        var v1 = GetStationValue(station, alarm.VariableMnemonic);
        if (!v1.HasValue) return false;

        bool c1 = Compare(alarm.ConditionOperator, v1.Value, alarm.ThresholdValue);
        bool c2 = true;
        if (!string.IsNullOrEmpty(alarm.SecondVariableMnemonic) && alarm.SecondThresholdValue.HasValue && !string.IsNullOrEmpty(alarm.SecondConditionOperator))
        {
            var v2 = GetStationValue(station, alarm.SecondVariableMnemonic);
            c2 = v2.HasValue && Compare(alarm.SecondConditionOperator!, v2.Value, alarm.SecondThresholdValue.Value);
        }

        var satisfied = c1 && c2;
        var was = alarm.LastConditionSatisfied;
        alarm.LastConditionSatisfied = satisfied;
        alarm.IsTriggered = satisfied;
        alarm.UpdatedAt = DateTime.UtcNow;

        if (satisfied && !was)
        {
            alarm.TimesTriggered++;
            alarm.LastTriggeredAt = DateTime.UtcNow;
            var disp = FormatPair(v1, string.IsNullOrEmpty(alarm.SecondVariableMnemonic)
                ? null
                : GetStationValue(station, alarm.SecondVariableMnemonic));
            alarm.LastTriggeredValueDisplay = disp;
            alarm.LastHoleDepthMd = station.Md;
            var ev = new WellAlarmEvent
            {
                WellAlarmId = alarm.Id,
                TriggeredAt = alarm.LastTriggeredAt.Value,
                ValuePrimary = v1,
                ValueSecondary = GetStationValue(station, alarm.SecondVariableMnemonic),
                HoleDepthMd = station.Md,
                Severity = alarm.Severity
            };
            _db.WellAlarmEvents.Add(ev);
            if (alarm.NotifyEmail && !string.IsNullOrEmpty(_fallbackRecipient))
            {
                var subject = $"[{SeverityLabel(alarm.Severity)}] ALARMA: {alarm.Name}";
                var body = $"Variable {alarm.VariableMnemonic} valor {v1} (umbral {alarm.ConditionOperator} {alarm.ThresholdValue}).";
                await _emailSender.SendAlarmEmailAsync(_fallbackRecipient, subject, body, ct);
            }
        }

        await _db.SaveChangesAsync(ct);
        return satisfied && !was;
    }

    private static string SeverityLabel(int s) => s switch { 1 => "CRÍTICO", 2 => "MAYOR", 3 => "MENOR", _ => "NORMAL" };

    private async Task AppendAuditAsync(int alarmId, string action, string? detail, string? userId, CancellationToken ct)
    {
        _db.WellAlarmAudits.Add(new WellAlarmAudit
        {
            WellAlarmId = alarmId,
            Action = action,
            Detail = detail,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task<TrajectoryStation?> GetLatestStationAsync(string wellUid, CancellationToken ct)
    {
        var trajUids = await _db.Trajectories
            .Where(t => t.WellUid != null && t.WellUid.ToLower() == WellKey(wellUid))
            .Select(t => t.Uid)
            .ToListAsync(ct);
        if (trajUids.Count == 0) return null;
        return await _db.TrajectoryStations
            .Where(s => trajUids.Contains(s.TrajectoryUid))
            .OrderByDescending(s => s.Md ?? 0)
            .FirstOrDefaultAsync(ct);
    }

    private static decimal? GetStationValue(TrajectoryStation s, string? mnemonic)
    {
        if (string.IsNullOrEmpty(mnemonic)) return null;
        return mnemonic.ToLowerInvariant() switch
        {
            "md" => s.Md,
            "tvd" => s.Tvd,
            "incl" => s.Incl,
            "azi" => s.Azi,
            "disp_ns" => s.DispNs,
            "disp_ew" => s.DispEw,
            "vert_sect" => s.VertSect,
            "dls" => s.Dls,
            _ => null
        };
    }

    private static bool Compare(string op, decimal value, decimal threshold)
    {
        op = op.Trim();
        if (op == ">") return value > threshold;
        if (op == "<") return value < threshold;
        if (op == "=") return Math.Abs(value - threshold) < 0.0001m;
        return false;
    }

    private static string FormatPair(decimal? a, decimal? b)
    {
        if (b == null) return a?.ToString(CultureInfo.InvariantCulture) ?? "";
        return $"{a?.ToString(CultureInfo.InvariantCulture)}/{b.Value.ToString(CultureInfo.InvariantCulture)}";
    }

    private async Task<bool> CanViewAsync(WellAlarm a, string subscriberKey, CancellationToken ct)
    {
        if (a.OwnerUserId == subscriberKey) return true;
        if (!a.IsPublic) return false;
        return await _db.WellAlarmSubscriptions
            .AnyAsync(s => s.WellAlarmId == a.Id && s.SubscriberKey == subscriberKey, ct);
    }

    private async Task<HashSet<int>> GetSubscriberSetAsync(string subscriberKey, CancellationToken ct)
    {
        var ids = await _db.WellAlarmSubscriptions
            .Where(s => s.SubscriberKey == subscriberKey)
            .Select(s => s.WellAlarmId)
            .ToListAsync(ct);
        return ids.ToHashSet();
    }

    private AlarmDto MapToDto(WellAlarm a, string subscriberKey, HashSet<int> subSet)
    {
        var isSubOnly = a.OwnerUserId != subscriberKey && subSet.Contains(a.Id);
        var dtos = new AlarmDto
        {
            Id = a.Id,
            WellUid = a.WellUid,
            Name = a.Name,
            VariableMnemonic = a.VariableMnemonic,
            ConditionOperator = a.ConditionOperator,
            ThresholdValue = a.ThresholdValue,
            Unit = a.Unit,
            SecondVariableMnemonic = a.SecondVariableMnemonic,
            SecondConditionOperator = a.SecondConditionOperator,
            SecondThresholdValue = a.SecondThresholdValue,
            Severity = a.Severity,
            NotifyEmail = a.NotifyEmail,
            NotifySms = a.NotifySms,
            ExtraPhone = a.ExtraPhone,
            IsPublic = a.IsPublic,
            IsEnabled = a.IsEnabled,
            OwnerUserId = a.OwnerUserId,
            IsTriggered = a.IsTriggered,
            TimesTriggered = a.TimesTriggered,
            LastTriggeredAt = a.LastTriggeredAt,
            LastTriggeredValueDisplay = a.LastTriggeredValueDisplay,
            LastHoleDepthMd = a.LastHoleDepthMd,
            UiSnoozeUntil = a.UiSnoozeUntil,
            IsSubscriptionOnly = isSubOnly,
            ConditionDisplay = $"{a.VariableMnemonic} {a.ConditionOperator} {a.ThresholdValue} {a.Unit}".Trim(),
            SecondConditionDisplay = string.IsNullOrEmpty(a.SecondVariableMnemonic)
                ? null
                : $"{a.SecondVariableMnemonic} {a.SecondConditionOperator} {a.SecondThresholdValue}",
            TimeElapsedDisplay = FormatElapsed(a.LastTriggeredAt)
        };
        return dtos;
    }

    private static string? FormatElapsed(DateTime? last)
    {
        if (!last.HasValue) return null;
        var d = DateTime.UtcNow - last.Value;
        if (d.TotalDays >= 1) return $"{(int)d.TotalDays} d";
        if (d.TotalHours >= 1) return $"{(int)d.TotalHours} h";
        if (d.TotalMinutes >= 1) return $"{(int)d.TotalMinutes} min";
        return $"{(int)d.TotalSeconds} s";
    }

    private static void NormalizeUpsert(AlarmUpsertDto dto)
    {
        var op = dto.ConditionOperator.Trim();
        if (op is not (">" or "<" or "="))
            dto.ConditionOperator = ">";
        else
            dto.ConditionOperator = op;
        if (!string.IsNullOrEmpty(dto.SecondConditionOperator))
        {
            var o2 = dto.SecondConditionOperator.Trim();
            dto.SecondConditionOperator = o2 is (">" or "<" or "=") ? o2 : ">";
        }
    }

    private static int ClampSeverity(int s) => s is >= 1 and <= 4 ? s : 2;

    private static string EscapeCsv(string s)
    {
        if (s.Contains('"') || s.Contains(',') || s.Contains('\n'))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }
}
