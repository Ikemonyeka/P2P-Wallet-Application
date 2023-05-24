using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DataObjects
{
    public class LoginView
    {
        public string message { get; set; } = string.Empty;
        public string data { get; set; } = string.Empty;
        public bool status { get; set; }


    }
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
