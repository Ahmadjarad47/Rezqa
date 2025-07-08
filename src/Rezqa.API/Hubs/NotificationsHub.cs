using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;
using Rezqa.Domain.Identity;

namespace Rezqa.API.Hubs
{

    public class NotificationsHub : Hub
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly PresenceTracker _tracker;
        private readonly UserManager<AppUsers> _userManager;

        public NotificationsHub(INotificationRepository notificationRepository, PresenceTracker tracker, UserManager<AppUsers> userManager)
        {
            _notificationRepository = notificationRepository;
            _tracker = tracker;
            _userManager = userManager;
        }
        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;

            // تحقق إن كان المستخدم مسجل الدخول (مخول)
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                // مستخدم مسجل الدخول
                await _tracker.UserConnected(userId, connectionId);

                // إرسال قائمة المتصلين للأدمن
                var admins = await _userManager.GetUsersInRoleAsync(RoleSeeder.Roles.Admin);
                var adminUser = admins.FirstOrDefault();
                if (adminUser != null)
                {
                    var adminConnections = await _tracker.GetConnectionForUser(adminUser.Id.ToString());
                    await Clients.Clients(adminConnections).SendAsync("onlineUsers", await _tracker.GetOnline());
                }
            }
            else
            {
                // مستخدم غير مسجل (زائر)
                await _tracker.UserConnected(connectionId, connectionId); // استخدم ConnectionId كمفتاح بديل

                // يمكن إرسال معلومات للزائر هنا إذا أردت
            }
        }
        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            await _tracker.UserDisConnected(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!, Context.ConnectionId);


            // ابحث عن المستخدمين الذين لديهم دور Admin
            var admins = await _userManager.GetUsersInRoleAsync(RoleSeeder.Roles.Admin);

            // تحقق إذا وجدنا على الأقل أدمن واحد
            var adminUser = admins.FirstOrDefault();
            if (adminUser == null)
                throw new Exception("No admin user found");

            await Clients.Clients(await _tracker.GetConnectionForUser
                (adminUser.Id.ToString())).SendAsync("onlineUsers", await _tracker.GetOnline());
        }

        [Authorize(Roles = "Admin")]
        public async Task SendAllUsersNotification(string title, string message)
        {

            var notification = new Notification
            {
                UserId = "",
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                Status = NotificationStatus.Read,
            };
            await Clients.All.SendAsync("ReceiveNotification", notification);
        }

        [Authorize(Roles = "Admin")]
        public async Task SendNotification(string userId, string title, string message)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("Recipient userId is required");

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                Status = NotificationStatus.Unread,
            };
            await _notificationRepository.AddAsync(notification);
            await Clients.Clients(await _tracker.GetConnectionForUser(userId)).SendAsync("ReceiveNotification", notification);
        }
        [Authorize]
        public async Task MarkAsRead(Guid notificationId)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        [Authorize(Roles = "Admin")]
        public async Task DeleteNotification(int notificationId)
        {
            await _notificationRepository.DeleteAsync(notificationId);
        }
        [Authorize]
        public async Task<List<Notification>> GetNotifications(string? status = null)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            NotificationStatus? notificationStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<NotificationStatus>(status, out var parsedStatus))
            {
                notificationStatus = parsedStatus;
            }

            var result = await _notificationRepository.GetByUserIdAsync(userId, notificationStatus);
            return result.Count == 0 ? new List<Notification>() : result;
        }
    }
}
