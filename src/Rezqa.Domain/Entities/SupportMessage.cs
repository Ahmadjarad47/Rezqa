using System;

namespace Rezqa.Domain.Entities
{
    public class SupportMessage
    {
        public SupportMessage()
        {
            
        }
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime sentAt { get; set; }
        public bool isFromAdmin { get; set; }
    }
}