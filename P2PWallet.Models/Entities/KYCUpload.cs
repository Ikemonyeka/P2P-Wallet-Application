using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class KYCUpload
    {
        public int id {  get; set; }
        public string PathName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public bool? Status { get; set; }
        public DateTime date { get; set; }

        [ForeignKey("User")]
        public int userId { get; set; }
        public virtual User User { get; set; }

        [ForeignKey("KYCRequiredDocuments")]
        public int KycRecId { get; set; }
        public virtual KYCRequiredDocuments KYCRequiredDocuments { get; set; }

    }
}
