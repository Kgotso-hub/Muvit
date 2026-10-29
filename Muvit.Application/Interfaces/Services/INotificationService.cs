namespace Muvit.Application.Interfaces.Services;

public interface INotificationService
{
    Task NotifyAsync(Guid recipientUserId, string subject, string message);
}