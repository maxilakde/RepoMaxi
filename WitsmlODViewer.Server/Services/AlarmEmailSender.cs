using Microsoft.Extensions.Options;

namespace WitsmlODViewer.Server.Services;

public interface IAlarmEmailSender
{
    Task SendAlarmEmailAsync(string to, string subject, string body, CancellationToken ct = default);
}

public class AlarmEmailSender : IAlarmEmailSender
{
    private readonly AlarmEmailOptions _options;

    public AlarmEmailSender(IOptions<AlarmEmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAlarmEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) || string.IsNullOrWhiteSpace(to))
            return;

        using var msg = new System.Net.Mail.MailMessage
        {
            From = new System.Net.Mail.MailAddress(_options.FromAddress ?? "noreply@localhost"),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        msg.To.Add(to);

        using var client = new System.Net.Mail.SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.SmtpUseSsl,
            Credentials = string.IsNullOrEmpty(_options.SmtpUser)
                ? null
                : new System.Net.NetworkCredential(_options.SmtpUser, _options.SmtpPassword)
        };
        await client.SendMailAsync(msg, ct);
    }
}
