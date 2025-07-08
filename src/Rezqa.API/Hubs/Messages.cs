using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Rezqa.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Rezqa.Domain.Identity;
using System.Collections.Generic;

namespace Rezqa.API.Hubs
{
    [Authorize]
    public class Messages : Hub
    {
        private readonly IMemoryCache _memoryCache;
        private const int CacheDurationDays = 3;
        private const string ConversationCacheKeyPrefix = "SupportConversation_";
        private readonly UserManager<AppUsers> _userManager;
        private readonly PresenceMessageTracker _tracker;
        private readonly PresenceTracker _Precencetracker;
        private readonly IHubContext<NotificationsHub> hubContext;
        private const string AllConversationKeysCacheKey = "AllConversationKeys";

        public Messages(IMemoryCache memoryCache, UserManager<AppUsers> userManager, PresenceMessageTracker tracker, PresenceTracker precencetracker, IHubContext<NotificationsHub> hubContext)
        {
            _memoryCache = memoryCache;
            _userManager = userManager;
            _tracker = tracker;
            _Precencetracker = precencetracker;
            this.hubContext = hubContext;
        }
        public override async Task OnConnectedAsync()
        {
            await _tracker.UserConnected(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!, Context.ConnectionId);

            if (!Context.User!.IsInRole("Admin"))
            {
                var senderId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

                var admins = await _userManager.GetUsersInRoleAsync(RoleSeeder.Roles.Admin);

                // تحقق إذا وجدنا على الأقل أدمن واحد
                var adminUser = admins.FirstOrDefault();
                if (adminUser == null)
                    throw new Exception("No admin user found");
                await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(senderId, adminUser.Id.ToString()));
            }
        }
        [Authorize(Roles = "Admin")]
        public async Task joinGroup(string id)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(id, Context.User.FindFirstValue(ClaimTypes.NameIdentifier)!));

        }
        public async Task SendNewMessage(string otherId, string message)
        {
            var senderId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (senderId == null) throw new Exception("Sender not found");

            // Get admin as receiver if sender is not admin
            if (!Context.User!.IsInRole("Admin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync(RoleSeeder.Roles.Admin);
                var adminUser = admins.FirstOrDefault();
                if (adminUser == null) throw new Exception("No admin user found");
                otherId = adminUser.Id.ToString();
            }
            Random random = new Random();
            var supportMessage = new SupportMessage
            {
                Id = random.Next(0, 102910231),
                Message = message,
                IsRead = false,
                SenderId = senderId!,
                ReceiverId = otherId,
                sentAt = DateTime.UtcNow,
                isFromAdmin = Context.User!.IsInRole("Admin")
            };

            var groupName = GetGroupName(senderId!, otherId);
            var cacheKey = GetConversationCacheKey(senderId!, otherId);

            // جلب الرسائل القديمة من الكاش أو إنشاء قائمة جديدة
            var messages = _memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(CacheDurationDays);
                return new List<SupportMessage>();
            });

            messages!.Add(supportMessage);
            _memoryCache.Set(cacheKey, messages, TimeSpan.FromDays(CacheDurationDays));

            // تخزين معرف المحادثة في القائمة العامة لتتبع المستخدمين الذين تم التواصل معهم
            var allKeys = _memoryCache.GetOrCreate(AllConversationKeysCacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(CacheDurationDays);
                return new HashSet<string>();   
            });

            allKeys!.Add(groupName);
            _memoryCache.Set(AllConversationKeysCacheKey, allKeys, TimeSpan.FromDays(CacheDurationDays));

            var connectionIds = await _tracker.GetConnectionForUser(otherId) ?? new List<string>();
            if (!connectionIds.Any())
            {
                connectionIds.AddRange(await _Precencetracker.GetConnectionForUser(otherId));
                await hubContext.Clients.Clients(connectionIds).SendAsync("ReceiveNotification",  supportMessage);
                await Clients.Group(groupName).SendAsync("ReceiveMessage", supportMessage);

            }
            else
            {
                supportMessage.IsRead = true;
                await Clients.Group(groupName).SendAsync("ReceiveMessage", supportMessage);
            }
        }
        private string GetConversationCacheKey(string userId1, string userId2)
        {
            var groupName = GetGroupName(userId1, userId2);
            return $"{ConversationCacheKeyPrefix}{groupName}";
        }
        public async Task<List<SupportMessage>> GetMessagesWithUser(string otherUserId)
        {
            if (!Context.User.IsInRole("Admin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync(RoleSeeder.Roles.Admin);
                var adminUser = admins.FirstOrDefault();
                if (adminUser == null) throw new Exception("No admin user found");
                otherUserId = adminUser.Id.ToString();
            }
            var currentUserId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                throw new HubException("User not authenticated");

            var cacheKey = GetConversationCacheKey(currentUserId, otherUserId);

            var messages = _memoryCache.Get<List<SupportMessage>>(cacheKey);
            return messages ?? new List<SupportMessage>();
        }

        [Authorize(Roles = "Admin")]
        public async Task<List<ContactedUserDto>> GetContactedUsersOrdered()
        {
            string adminId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var allKeys = _memoryCache.Get<HashSet<string>>(AllConversationKeysCacheKey);
            var result = new List<ContactedUserDto>();

            if (allKeys == null)
                return result;

            foreach (var key in allKeys)
            {
                var ids = key.Replace(ConversationCacheKeyPrefix, "").Split('_');
                if (!ids.Contains(adminId)) continue;

                var userId = ids.First(id => id != adminId);
                var cacheKey = GetConversationCacheKey(adminId, userId);
                var messages = _memoryCache.Get<List<SupportMessage>>(cacheKey);
                if (messages == null || messages.Count == 0) continue;

                bool hasUnread = messages.Any(m => m.SenderId == userId && !m.IsRead);
                DateTime lastMessageTime = messages.Max(m => m.sentAt);

                result.Add(new ContactedUserDto
                {
                    UserId = userId,
                    HasUnreadMessages = hasUnread,
                    LastMessageTime = lastMessageTime
                });
            }

            var ordered = result
                //.OrderByDescending(x => x.HasUnreadMessages)
                //.ThenByDescending(x => x.LastMessageTime)
                .ToList();

            return ordered;
        }

        [Authorize(Roles = "Admin")]
        public async Task<bool> DeleteMessage(string otherUserId, int messageId)
        {
            var adminId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var cacheKey = GetConversationCacheKey(adminId, otherUserId);
            var messages = _memoryCache.Get<List<SupportMessage>>(cacheKey);

            if (messages == null || messages.Count == 0)
                return false;

            // Find the message to delete
            var messageToDelete = messages.FirstOrDefault(m => m.Id == messageId);
            if (messageToDelete == null)
                return false;

            // Remove the message from the list
            messages.Remove(messageToDelete);

            // Update the cache
            _memoryCache.Set(cacheKey, messages, TimeSpan.FromDays(CacheDurationDays));

            // Notify all clients in the group about the deleted message
            await Clients.Group(GetGroupName(adminId, otherUserId)).SendAsync("MessageDeleted", messageId);

            return true;
        }

        public async Task MarkMessagesAsRead(string otherUserId)
        {
            var currentUserId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                throw new HubException("User not authenticated");

            var cacheKey = GetConversationCacheKey(currentUserId, otherUserId);

            var messages = _memoryCache.Get<List<SupportMessage>>(cacheKey);
            if (messages == null || messages.Count == 0)
                return;

            // تعليم الرسائل التي أرسلها الطرف الآخر كمقروءة
            foreach (var msg in messages)
            {
                if (msg.SenderId == otherUserId)
                    msg.IsRead = true;
            }

            // تحديث الكاش بنفس المفتاح والمدة
            _memoryCache.Set(cacheKey, messages, TimeSpan.FromDays(CacheDurationDays));

            // إذا تريد يمكنك إرسال إشعار بتحديث حالة القراءة هنا
            await Clients.Group(GetGroupName(currentUserId, otherUserId)).SendAsync("ReceiveMessage", otherUserId);
        }

        private string GetGroupName(string userId, string otherUserId)
        {
            return string.CompareOrdinal(userId, otherUserId) < 0 ? $"{userId}_{otherUserId}" : $"{otherUserId}_{userId}";
        }
    }
    public class ContactedUserDto
    {
        public ContactedUserDto()
        {

        }
        public string UserId { get; set; } = null!;
        public bool HasUnreadMessages { get; set; }
        public DateTime LastMessageTime { get; set; }
    }

}
