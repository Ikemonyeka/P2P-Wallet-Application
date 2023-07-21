using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class SecurityQuestion
    {
        public int Id { get; set; }
        public string? SecurityQ { get; set; }
        public string? SecurityA { get; set; }

        [ForeignKey("User")]
        public int userId { get; set; }
        public virtual User User { get; set; }
    }
}
