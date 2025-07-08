using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;
using Rezqa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezqa.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;
        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Set<Notification>().AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<Notification> GetByIdAsync(Guid id)
        {
            return await _context.Set<Notification>().FindAsync(id);
        }

        public async Task<List<Notification>> GetByUserIdAsync(string userId, NotificationStatus? status = null)
        {
            var query = _context.Set<Notification>().Where(n => n.UserId == userId);
            if (status.HasValue)
                query = query.Where(n => n.Status == status.Value);
            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            var notification = await _context.Set<Notification>().FindAsync(id);
            if (notification != null && notification.Status != NotificationStatus.Read)
            {
                notification.Status = NotificationStatus.Read;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = _context.Set<Notification>().Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread);
            await notifications.ForEachAsync(n => n.Status = NotificationStatus.Read);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var notification = await _context.Set<Notification>().FindAsync(id);
            if (notification != null)
            {
                _context.Set<Notification>().Remove(notification);
                await _context.SaveChangesAsync();
            }
        }
    }
} 