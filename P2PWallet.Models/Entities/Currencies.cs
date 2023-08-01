using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class Currencies
    {
        public int Id { get; set; }
        public string Currency { get; set; }
        public decimal conversionRate { get; set; }
        public decimal chargeRate { get; set; }
    }
}
