namespace RealWorldApp.NotificationService.Contracts;
public interface INotificationService
{
    Task NotifyArticleCreatedAsync(Guid articleId, string title, string author);
    Task NotifyArticleUpdatedAsync(Guid articleId, string title);
}