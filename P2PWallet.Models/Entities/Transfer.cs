using P2PWallet.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class Transfer
    {
        public int Id { get; set; }
        public string? DebitAccountNo { get; set; }
        public string? BeneficiaryAccountNo { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int? DebitUserId { get; set; }
        public int? BeneficiaryUserId { get; set; }

        [ForeignKey("DebitUserId")]
        [InverseProperty("Transfers")]
        public virtual User DebitUser { get; set; }

        [ForeignKey("BeneficiaryUserId")]
        public virtual User BeneficiaryUser { get; set; }

    }
}





