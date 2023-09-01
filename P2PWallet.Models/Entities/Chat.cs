using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class Chat
    {
        public int Id { get; set; }
        public string message { get; set; } = string.Empty;
        public DateTime date { get; set; }
        public string messageType { get; set; } = string.Empty;
        public bool isReadUser { get; set; }
        public bool isReadAdmin { get; set;}

        [ForeignKey("User")]
        public int userId { get; set; }
        public virtual User User { get; set; }

        [ForeignKey("Admin")]
        public int? adminId { get; set; }
        public virtual Admin? Admin { get; set; }
    }
}
