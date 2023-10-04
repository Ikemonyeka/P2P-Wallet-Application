using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class KYCRequiredDocuments
    {
        public int id { get; set; }
        public string NameOfDocument { get; set; } = string.Empty;
        public string FormCode { get; set; } = string.Empty;
        public bool isEnabled { get; set; }
        public virtual List<KYCUpload> KYCUploads { get; set; }
    }
}
