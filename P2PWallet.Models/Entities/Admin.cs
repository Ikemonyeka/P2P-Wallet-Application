using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class Admin
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }  
        public string Email { get; set; } = string.Empty;
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string phoneNumber { get; set; } = string.Empty;
        public bool Status { get; set; } 
        public DateTime? LastLogin { get; set; }
        public bool IsLoggedIn { get; set; }
        public virtual List<Chat> Chats { get; set; }
    }
}
