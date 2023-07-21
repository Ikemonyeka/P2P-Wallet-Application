using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DataObjects
{
    public class PdfDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string File { get; set; } = string.Empty;
    }

    public class PdfView
    {
        public string CUsername { get; set; } = string.Empty;
        public string DUsername { get; set; } = string.Empty;
        public string? BeneficiaryAccountNo { get; set; } = string.Empty;
        public int? DebitUserId { get; set; }
        public int? BeneficiaryUserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string TransactionType { get; set; }
    }

    public class PdfInfo
    {
        public string AccountNo { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        //public decimal OBalance { get; set; }
        public decimal CBalance { get; set; }
    }

    public class PdfDate
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
