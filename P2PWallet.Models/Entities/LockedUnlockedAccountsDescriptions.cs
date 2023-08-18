using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class LockedUnlockedAccountsDescriptions
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        [ForeignKey("User")]
        public int userId { get; set; }
        public virtual User User { get; set; }

        [ForeignKey("LockedUnlockedUser")]
        public int DescriptionId { get; set; }
        public virtual LockedUnlockedUserDescription LockedUnlockedUser { get; set; }
    }
}
