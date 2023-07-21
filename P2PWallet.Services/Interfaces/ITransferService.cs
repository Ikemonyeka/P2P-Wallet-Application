global using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace P2PWallet.Services.Interfaces
{
    public interface ITransferService
    {
        Task<LoginView> Transaction(TransferDto transfer);
        Task<ActionResult<object>> TransactionVerify(string transfer);
        Task<object> GetDebitUser();
        Task<List<TransferView>> GetTransferHistory();
        Task<List<TransferView>> GetTransferHistoryByDate(PdfDto pdfDto);
    }
}
