using System;

namespace ServiceDesk.Shared.Entities
{
    public class ChatMessage
    {
        public string Text { get; set; } = string.Empty;
        public bool IsUser { get; set; }
        public bool IsError { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsAi => !IsUser;
    }
}