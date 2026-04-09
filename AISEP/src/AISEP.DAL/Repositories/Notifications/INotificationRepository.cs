using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Notifications
{
    public interface INotificationRepository
    {
        IQueryable<Notification> GetByUserIdQuery(int userId);
        Task AddAsync(Notification notification);
        Task<Notification?> GetByIdAsync(int notificationId);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<int> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteAsync(int notificationId, int userId);
    }
}
