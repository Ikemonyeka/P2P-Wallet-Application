using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public string AccountNo { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;

        [ForeignKey("User")]
        public int userId { get; set; }
        public virtual User User { get; set; }
    }
}