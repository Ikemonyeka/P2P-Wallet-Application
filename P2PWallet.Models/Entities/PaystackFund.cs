using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class PaystackFund
    {
        public int Id {  get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "NGN";
        public string Reference { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime Date { get; set; }

        [ForeignKey ("User")]
        public int userId { get; set; }
        public virtual User User { get; set; }
        public PaystackFund()
        {
            this.Date = DateTime.Now;
        }
    }
}
