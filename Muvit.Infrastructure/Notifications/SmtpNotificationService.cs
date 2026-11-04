using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Muvit.Application.Interfaces.Repositories;
using Muvit.Application.Interfaces.Services;

namespace Muvit.Infrastructure.Notifications;

public sealed class SmtpNotificationService : INotificationService
{
    private readonly IUserRepository _userRepository;
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromAddress;
    private readonly string _fromName;
    private readonly bool _enableSsl;

    public SmtpNotificationService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _host = configuration["Smtp:Host"]!;
        _port = int.Parse(configuration["Smtp:Port"]!);
        _username = configuration["Smtp:Username"]!;
        _password = configuration["Smtp:Password"]!;
        _fromAddress = configuration["Smtp:FromAddress"]!;
        _fromName = configuration["Smtp:FromName"] ?? "Muvit";
        _enableSsl = bool.Parse(configuration["Smtp:EnableSsl"] ?? "true");
    }

    public async Task NotifyAsync(Guid recipientUserId, string subject, string message)
    {
        var recipient = await _userRepository.GetByIdAsync(recipientUserId);
        if (recipient is null)
            return; // nothing to notify — a missing user shouldn't blow up the caller's workflow

        try
        {
            using var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_username, _password),
                EnableSsl = _enableSsl
            };

            using var mail = new MailMessage(new MailAddress(_fromAddress, _fromName), new MailAddress(recipient.Email))
            {
                Subject = subject,
                Body = message
            };

            await client.SendMailAsync(mail);
        }
        catch (SmtpException)
        {
            // Notifications are a best-effort side effect, not a core transaction step — an SMTP
            // outage or bad credentials should never block a booking status update. Swallowed
            // deliberately per the ticket #30 scope call; a real deployment would log this via
            // ILogger (no logging infrastructure exists yet in this project) rather than go silent.
        }
    }
}