using Rezqa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rezqa.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<Notification> GetByIdAsync(Guid id);
        Task<List<Notification>> GetByUserIdAsync(string userId, NotificationStatus? status = null);
        Task MarkAsReadAsync(Guid id);
        Task MarkAllAsReadAsync(string userId);
        Task DeleteAsync(int id);
    }
} 