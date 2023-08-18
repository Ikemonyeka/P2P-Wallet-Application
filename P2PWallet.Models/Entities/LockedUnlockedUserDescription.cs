using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class LockedUnlockedUserDescription
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public virtual List<LockedUnlockedAccountsDescriptions> LockedUnlockedDescriptions { get; set; }
    }
}
