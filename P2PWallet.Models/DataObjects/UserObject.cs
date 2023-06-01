using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DataObjects
{
    public class UserObject
    {
        public class UserView
        {
            public string message { get; set; } = string.Empty;
            public string data { get; set; } 
            public bool status { get; set; }

        }
        public class UserDto
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string cPassword { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string firstName { get; set; } = string.Empty;
            public string lastName { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
        }

        public class PinDto
        {
            public string Pin { get; set; } = string.Empty;
            //public string cPin { get; set; } = string.Empty;
        }

        public class EmailDto
        {
            public string Email { get; set; }
        }

        public class ResetPassword
        {
            public string Email { get; set; }
            public string Token { get; set; }
            public string Password { get; set; }
            public string cPassword { get; set; }
        }

        public class UserProfile
        {
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
        }
    }
}
