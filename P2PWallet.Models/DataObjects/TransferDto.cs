using P2PWallet.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DataObjects
{
    public class TransferView
    {
        public string CUsername { get; set; } = string.Empty;
        public string DUsername { get; set; } = string.Empty;
        public string? BeneficiaryAccountNo { get; set; } = string.Empty;
        public int? DebitUserId { get; set; }
        public int? BeneficiaryUserId { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Date { get; set; }
        public string TransactionType { get; set; }
    }
    public class TransferDto
    {
        public string SourceAccountNo { get; set; } = string.Empty;
        public string? BeneficiaryAccountNo { get; set; } = string.Empty;
        public decimal Amount { get; set; }

    }

    public class TransferVerify
    {
        public string SourceAccountNo { get; set; } = string.Empty;
        public string? BeneficiaryAccountNo { get; set; } = string.Empty;

    }

    public class RecieverDetails
    {
        public string ReceiverName { get; set; }
        public string ReceiverAccountNo { get; set; }
    }

    public class TransferSDto
    {
        public string AccountNo { get; set; }
        public decimal Amount { get; set; }
    }
}
