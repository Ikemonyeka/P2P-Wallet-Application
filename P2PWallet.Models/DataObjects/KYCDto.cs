using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.AdminDto;

namespace P2PWallet.Models.DataObjects
{
    public class KYCDto
    {
        public class NewKYCField
        {
            public string documentName { get; set; } = string.Empty;
        }

        public class ResponseMessage
        {
            public string message { get; set; }
            public string dt { get; set; }
            public bool status { get; set; }
        }

        public class ResponseMessageModel<T> : ResponseMessage
        {
            public T data { get; set; }
        }
        public class newUpload
        {
            public string formCode { get; set; } = string.Empty;
            public IFormFile file { get; set; }
        }

        public class AdminKYCView
        {
            public string username { get; set; } = string.Empty;
            public string formCode { get; set; } = string.Empty;
            public int pendingCount { get; set; }
            public string file { get; set; }
            public byte[]? pendingFile { get; set; }
        }

        public class AcceptUserUpload
        {
            public string username { get; set; } = string.Empty;
            public string formCode { get; set; } = string.Empty;
        }

        public class RejectUserUpload
        {
            public string username { get; set; } = string.Empty;
            public string formCode { get; set; } = string.Empty;
            public string reason { get; set; } = string.Empty;
        }

        public class Pagination
        {
            public Dictionary<string, List<AdminKYCView>> users { get; set; } = new Dictionary<string, List<AdminKYCView>>();
            public int pages { get; set; }
            public int currentpage { get; set; }
        }

        public class PaginationUserCards
        {
            public Pagination Pagination { get; set; }
        }
    }
}
