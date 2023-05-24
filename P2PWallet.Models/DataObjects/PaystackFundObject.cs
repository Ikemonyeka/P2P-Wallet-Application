using P2PWallet.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DataObjects
{
    public class PaystackFundObject
    {
        public class PaystackFundView
        {
            public string amount { get; set; }
            public string reference { get; set; }
            public string currency { get; set; } = "NGN";
            public string email { get; set; }
        }


        public class PaystackFundDto
        {
            public string message { get; set; } = string.Empty;
            public string data { get; set; } = string.Empty;
            public bool status { get; set; }
        }

        public class PaystackFundModel
        {
            public bool status { get; set; }
            public string message { get; set; }
            public Data data { get; set; }
        }

        public class Data
        {
            public string authorization_url { get; set; }
            public string access_code { get; set; }
            public string reference { get; set; }
        }
    }
}
