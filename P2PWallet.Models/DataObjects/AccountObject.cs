using P2PWallet.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DataObjects
{
    public class AccountObject
    {
        public class AccountView
        {
        }
        public class AccountDto
        {
            public string AccountNo { get; set; } = string.Empty;
            public decimal Balance { get; set; }
            public string Currency { get; set; } = string.Empty;
            public virtual User User { get; set; }

        }
    }
}
