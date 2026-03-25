using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Notifications
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetByUserIdAsync(int userId, int pageIndex, int pageSize);
        Task AddAsync(Notification notification);
        Task<Notification?> GetByIdAsync(int notificationId);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<int> MarkAllAsReadAsync(int userId);
    }
}
