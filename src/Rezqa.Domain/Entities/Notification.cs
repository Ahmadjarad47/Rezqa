using System;

namespace Rezqa.Domain.Entities
{
    public enum NotificationStatus
    {
        Unread = 0,
        Read = 1
    }

    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public NotificationStatus Status { get; set; }
    }
}