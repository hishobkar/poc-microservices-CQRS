using System.Threading;
using System.Threading.Tasks;

namespace RealWorldApp.NotificationService.Contracts;

public interface IEmailService
{
    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = false,
        CancellationToken cancellationToken = default);
}
