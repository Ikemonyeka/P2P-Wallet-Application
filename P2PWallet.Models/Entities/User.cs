using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class User
    {
        public int userId { get; set; }
        public string Username { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public byte[]? PinHash { get; set; }
        public byte[]? PinSalt { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? VerificationToken { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public string PhoneNumber { get; set; } = string.Empty; 
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public byte[]? ProfilePhoto { get; set; }
        public virtual List<Account> Account { get; set; }
        public virtual SecurityQuestion SecurityQuestions { get; set; }
        public virtual List<Transfer> Transfers { get; set; }
        public virtual List<PaystackFund> PaystackFunds { get; set;}
        public virtual List<Notifications> Notifications { get; set; }
    }
}
