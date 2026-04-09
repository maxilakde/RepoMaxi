using WitsmlODViewer.Server.DTOs;
using WitsmlODViewer.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace WitsmlODViewer.Server.Controllers;

[ApiController]
[Route("api/v1/wells/{wellUid}/alarms")]
public class WellAlarmsController : ControllerBase
{
    private const string SubscriberHeader = "X-Subscriber-Key";
    private readonly IAlarmsService _alarms;

    public WellAlarmsController(IAlarmsService alarms)
    {
        _alarms = alarms;
    }

    private string Subscriber =>
        Request.Query["subscriberKey"].FirstOrDefault()
        ?? Request.Headers[SubscriberHeader].FirstOrDefault()
        ?? "default";

    [HttpGet]
    public Task<IReadOnlyList<AlarmDto>> List(string wellUid, CancellationToken ct) =>
        _alarms.ListAsync(wellUid, Subscriber, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AlarmDto>> Get(string wellUid, int id, CancellationToken ct)
    {
        var a = await _alarms.GetAsync(wellUid, id, Subscriber, ct);
        return a == null ? NotFound() : Ok(a);
    }

    [HttpPost]
    public Task<AlarmDto> Create(string wellUid, [FromBody] AlarmUpsertDto dto, CancellationToken ct) =>
        _alarms.CreateAsync(wellUid, dto, Subscriber, ct);

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AlarmDto>> Update(string wellUid, int id, [FromBody] AlarmUpsertDto dto, CancellationToken ct)
    {
        var a = await _alarms.UpdateAsync(wellUid, id, dto, Subscriber, ct);
        return a == null ? NotFound() : Ok(a);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(string wellUid, int id, CancellationToken ct)
    {
        var ok = await _alarms.DeleteAsync(wellUid, id, Subscriber, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/enable")]
    public async Task<ActionResult> Enable(string wellUid, int id, CancellationToken ct)
    {
        var ok = await _alarms.SetEnabledAsync(wellUid, id, true, Subscriber, ct);
        return ok ? Ok() : NotFound();
    }

    [HttpPost("{id:int}/disable")]
    public async Task<ActionResult> Disable(string wellUid, int id, CancellationToken ct)
    {
        var ok = await _alarms.SetEnabledAsync(wellUid, id, false, Subscriber, ct);
        return ok ? Ok() : NotFound();
    }

    [HttpGet("{id:int}/events")]
    public Task<PagedAlarmEventsDto> Events(string wellUid, int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default) =>
        _alarms.GetEventsAsync(wellUid, id, page, pageSize, Subscriber, ct);

    [HttpGet("{id:int}/events/export")]
    public async Task<IActionResult> ExportEventsCsv(string wellUid, int id, CancellationToken ct)
    {
        var bytes = await _alarms.ExportEventsToCsvAsync(wellUid, id, Subscriber, ct);
        if (bytes.Length == 0) return NotFound();
        return File(bytes, "text/csv; charset=utf-8", $"alarm-{id}-events.csv");
    }

    [HttpPost("evaluate")]
    public Task<EvaluateAlarmsResultDto> Evaluate(string wellUid, CancellationToken ct) =>
        _alarms.EvaluateWellAsync(wellUid, ct);

    [HttpGet("public-candidates")]
    public Task<IReadOnlyList<AlarmDto>> PublicCandidates(string wellUid, CancellationToken ct) =>
        _alarms.ListPublicCandidatesAsync(wellUid, Subscriber, ct);

    [HttpPost("{id:int}/subscribe")]
    public async Task<ActionResult> Subscribe(string wellUid, int id, [FromBody] SubscribeBody? body, CancellationToken ct)
    {
        var ok = await _alarms.SubscribeAsync(wellUid, id, Subscriber, body?.NotifyEmail ?? true, body?.NotifySms ?? false, ct);
        return ok ? Ok() : BadRequest();
    }

    [HttpPost("{id:int}/unsubscribe")]
    public async Task<ActionResult> Unsubscribe(string wellUid, int id, CancellationToken ct)
    {
        var ok = await _alarms.UnsubscribeAsync(wellUid, id, Subscriber, ct);
        return ok ? Ok() : NotFound();
    }

    [HttpGet("notifications/pending")]
    public Task<IReadOnlyList<AlarmNotificationDto>> PendingNotifications(string wellUid, CancellationToken ct) =>
        _alarms.GetPendingUiNotificationsAsync(wellUid, Subscriber, ct);

    [HttpPost("{id:int}/snooze")]
    public Task Snooze(string wellUid, int id, [FromBody] SnoozeBody? body, CancellationToken ct) =>
        _alarms.SnoozeAlarmAsync(wellUid, id, body?.Minutes ?? 5, Subscriber, ct);

    [HttpPost("{id:int}/dismiss-notification")]
    public Task DismissNotification(string wellUid, int id, CancellationToken ct) =>
        _alarms.DismissUiNotificationAsync(wellUid, id, Subscriber, ct);

    [HttpGet("{id:int}/export/xml")]
    public async Task<IActionResult> ExportXml(string wellUid, int id, CancellationToken ct = default)
    {
        var xml = await _alarms.ExportAlarmToXmlAsync(wellUid, id, Subscriber, ct);
        if (string.IsNullOrEmpty(xml)) return NotFound();
        var bytes = System.Text.Encoding.UTF8.GetBytes(xml);
        return File(bytes, "application/xml", $"alarm-{id}.xml");
    }

    [HttpPost("import/xml")]
    public Task<IReadOnlyList<AlarmDto>> ImportXml(string wellUid, [FromBody] ImportXmlBody body, CancellationToken ct) =>
        _alarms.ImportAlarmsFromXmlAsync(wellUid, body.Xml ?? "", Subscriber, ct);

    public class SubscribeBody
    {
        public bool NotifyEmail { get; set; } = true;
        public bool NotifySms { get; set; }
    }

    public class SnoozeBody
    {
        public int Minutes { get; set; } = 5;
    }

    public class ImportXmlBody
    {
        public string? Xml { get; set; }
    }
}
