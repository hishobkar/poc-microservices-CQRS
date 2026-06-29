using RealWorldApp.NotificationService.Contracts;

namespace RealWorldApp.NotificationService.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending email to {To}: Subject: {Subject}, Body: {Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
