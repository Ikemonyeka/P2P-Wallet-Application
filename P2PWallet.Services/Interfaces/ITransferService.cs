﻿global using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using static P2PWallet.Models.DataObjects.UserObject;

namespace P2PWallet.Services.Interfaces
{
    public interface ITransferService
    {
        Task<LoginView> Transaction(TransferDto transfer);
        Task<ActionResult<object>> TransactionVerify(TransferVerify transferVerify);
        Task<object> GetDebitUser();
        Task<List<TransferView>> GetTransferHistory();
        Task<List<TransferView>> GetTransferHistoryByDate(PdfDto pdfDto);
        Task<UserView> FundForeignCurrrency(TransferSDto transfer);
    }
}
