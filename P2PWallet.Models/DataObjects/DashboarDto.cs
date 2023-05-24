using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DataObjects
{
    public class DashboardView
    {
        public string Username { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class DashboardDto
    {
        public string message { get; set; } = string.Empty;
        public bool status { get; set; }
    }
}
