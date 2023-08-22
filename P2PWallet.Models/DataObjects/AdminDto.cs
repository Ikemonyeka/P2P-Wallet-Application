using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DataObjects
{
    public class AdminDto
    {
        public class AdminR
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string cPassword { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string firstName { get; set; } = string.Empty;
            public string lastName { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
        }

        public class ResponseObject
        {
            public string message { get; set; } = string.Empty;
            public string data { get; set; } = string.Empty;
            public bool status { get; set; }
        }

        public class ResponseMessage
        {
            public string message { get; set; }
            public string dt { get; set; }
            public bool status { get; set; }
        }

        public class ResponseMessageModel<T> : ResponseMessage
        {
            public T data { get; set; }
        }

        public class AdminLogin
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class AdminClaims
        {
            public int userId { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        public class LoggedInAdmin
        {
            public string Username { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string TimeOfDay { get; set; } = string.Empty;
        }

        public class allUsers
        {
            public string user { get; set; } = string.Empty;
            public string email { get; set; } = string.Empty;
            public bool status { get; set; }
            public string dateCreated { get; set; }
        }

        public class profileStatus
        {
            public string user { get; set; } = string.Empty;
            public string email { get; set; } = string.Empty;
            public bool status { get; set; }
        }

        public class DescriptionLU
        {
            public string user { get; set; } = string.Empty;
            public string email { get; set; } = string.Empty;
            public string description { get; set; } = string.Empty;
            public bool status { get; set; }
        }

        public class AdminPassword
        {
            public string username { get; set; } = string.Empty;
            public string password { get; set; } = string.Empty;
            public string cpassword { get; set; } = string.Empty;
        }
    }
}
