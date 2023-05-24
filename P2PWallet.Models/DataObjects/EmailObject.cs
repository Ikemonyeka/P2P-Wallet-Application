using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DataObjects
{
    public class SenderEmailDto
    {
        public string senderEmail { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string DUsername { get; set; }
        public string Date { get; set; }
        public string Reference { get; set; }
        public string CreditName { get; set; }
        public string TransferAmount { get; set; }
        public string CurrentBalance { get; set; }
    }

    public class ReceiverEmailDto
    {
        public string receiverEmail { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string CUsername { get; set; }
        public string Date { get; set; }
        public string Reference { get; set; }
        public string DebitName { get; set; }
        public string TransferAmount { get; set; }
        public string CurrentBalance { get; set; }
    }
}
