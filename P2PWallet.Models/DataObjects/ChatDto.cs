using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DataObjects
{
    public class ChatDto
    {
        public class ChatModel
        {
            public string message { get; set; } = string.Empty;
        }

        public class ChatModelAdmin
        {
            public string message { get; set; } = string.Empty;
            public string username { get; set; } = string.Empty;
        }

        public class ChatMessage
        {
            public string message { get; set; } = string.Empty;
            public string date { get; set; } = string.Empty;
            public int userId { get; set; }
            public string messageType {get; set;} = string.Empty;
        }

        public class UnreadMessage
        {
            public string message { get; set; } = string.Empty;
            public string date { get; set; } = string.Empty;
            public int userId { get; set; }
            public string messageType { get; set; } = string.Empty;
            public string username { get; set; } = string.Empty;
        }

        public class ChatsDto
        {
            public string message { get; set; } = string.Empty;
            public string date { get; set; } = string.Empty;
            public string messageType { get; set; } = string.Empty;
            public bool isReadAdmin { get; set; }
        }
    }
}
