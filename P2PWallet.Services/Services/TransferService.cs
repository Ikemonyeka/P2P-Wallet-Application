using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;     
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.WebPages;
using static P2PWallet.Models.DataObjects.PaystackFundObject;
using static P2PWallet.Models.DataObjects.UserObject;

namespace P2PWallet.Services.Services
{
    public class TransferService : ITransferService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly ILogger<TransferService> _logger;

        public TransferService(DataContext context, IHttpContextAccessor httpContextAccessor, IEmailService emailService,ILogger<TransferService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _logger = logger;
            _logger.LogDebug("NLog injected into TransferService");
        }

        private static string ReferenceGen()
        {
            const string src = "abcdefghijklmnopqrstuvwxyz0123456789";
            int length = 16;
            var sb = new StringBuilder();
            Random RNG = new Random();
            for (var i = 0; i < length; i++)
            {
                var c = src[RNG.Next(0, src.Length)];
                sb.Append(c);
            }
            return sb.ToString();
        }

        public async Task<List<TransferView>> GetTransferHistory()
        {
            TransferView view = new TransferView();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new List<TransferView>();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var transferData = await _context.Transfers
                .Where(transferInfo => transferInfo.DebitUserId == userID || transferInfo.BeneficiaryUserId == userID)
                .Select(transferInfo => new TransferView
                {
                    Date = transferInfo.Date.ToString("MM/dd/yyyy"),
                    BeneficiaryAccountNo = transferInfo.BeneficiaryAccountNo,
                    DUsername = transferInfo.DebitUser.firstName + " " + transferInfo.DebitUser.lastName,
                    CUsername = transferInfo.BeneficiaryUser.firstName + " " + transferInfo.BeneficiaryUser.lastName,
                    Amount = transferInfo.Amount,
                    Status = transferInfo.Status,
                    DebitUserId = transferInfo.DebitUserId,
                    BeneficiaryUserId = transferInfo.BeneficiaryUserId
                })
                .ToListAsync();

                foreach (var transferInfos in transferData)
                {
                     if (transferInfos.DebitUserId == null && transferInfos.BeneficiaryUserId == userID)
                    {
                        transferInfos.TransactionType = "C";
                        transferInfos.DUsername = "Paystack Funding";
                    }
                    if ( transferInfos.DebitUserId == userID)
                    {
                        transferInfos.TransactionType = "D";
                    }
                    else 
                    { 
                        transferInfos.TransactionType = "C"; 
                    }   
                }





                if (transferData == null)
                {
                    return new List<TransferView>();
                }

                return transferData;
            }
            catch (Exception ex)
            {

                _logger.LogError($"Check Backend - Transfer History \n {ex.Message}");
                return new List<TransferView>();
            }
        }

        public async Task<object> GetDebitUser()
        {
            LoginView view = new LoginView();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new LoginView();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);


                var userData = await _context.Users
                .Where(userInfo => userInfo.userId == userID)
                .Select(userInfo => new DashboardView
                {
                    AccountNo = userInfo.Account.AccountNo,
                    Balance = userInfo.Account.Balance,
                    Currency = userInfo.Account.Currency
                })
                .FirstOrDefaultAsync();


                if (userData == null) return new DashboardDto();


                return userData;
            }
            catch (Exception ex)
            {
                view.status = false;
                view.message = "Check Backend - Debit User Details";

                _logger.LogError($"{view.message} \n {ex.Message}");

                return view;
            }
        }


        public async Task<LoginView> Transaction(TransferDto transfer)
        {
            List<TransferDto> transferss = new List<TransferDto>();
            User users = new User();
            LoginView transfers = new LoginView();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new LoginView();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var data = await _context.Accounts.Where(checkId => checkId.Id == userID).FirstAsync();
                if (data == null)
                {
                    return transfers;
                }
                var sendEmail = await _context.Users
                .Where(r => r.userId == userID).FirstAsync();

                var data2 = await _context.Accounts.Where(checkAcc => checkAcc.AccountNo == transfer.BeneficiaryAccountNo).FirstAsync();
                if (data2 == null)
                {
                    return transfers;
                }

                var receivEmail = await _context.Users
                .Where(r => r.Account.AccountNo == transfer.BeneficiaryAccountNo).FirstAsync();

                var failedDTransfer = data.Balance;
                var failedCTransfer = data2.Balance;

                if (transfer.Amount > data.Balance)
                {
                    transfers.message = "Insufficient Funds";
                    transfers.status = false;
                    return transfers;
                }

                if (transfer.Amount <= 0)
                {
                    transfers.message = "Cannot transfer zero or a negative value";
                    transfers.status = false;
                    return transfers;
                }

                Transfer newTransfer = new Transfer
                {
                    DebitAccountNo = data.AccountNo,
                    BeneficiaryAccountNo = transfer.BeneficiaryAccountNo,
                    Amount = transfer.Amount,
                    Reference = ReferenceGen(),
                    Status = "Successful",
                    Date = DateTime.UtcNow,
                    DebitUserId = userID,
                    BeneficiaryUserId = data2.userId
                };

                data.Balance = data.Balance - transfer.Amount;
                data2.Balance = data2.Balance + transfer.Amount;

                if (failedDTransfer == data.Balance && failedCTransfer == data2.Balance)
                {
                    newTransfer.Status = "Transaction Failed";
                }

                await _context.Transfers.AddAsync(newTransfer);

                SenderEmailDto senderEmailDto = new SenderEmailDto();
                ReceiverEmailDto receiverEmailDto = new ReceiverEmailDto();

                var emailDate = newTransfer.Date.ToString("MM/dd/yyyy hh:mm tt");

                senderEmailDto.senderEmail = sendEmail.Email;
                senderEmailDto.Subject = newTransfer.Amount.ToString();
                senderEmailDto.DUsername = sendEmail.Username;
                senderEmailDto.Date = emailDate;
                senderEmailDto.Reference = newTransfer.Reference;
                senderEmailDto.CreditName = receivEmail.firstName + " " + receivEmail.lastName;
                senderEmailDto.TransferAmount = newTransfer.Amount.ToString();
                senderEmailDto.CurrentBalance = data.Balance.ToString();


                receiverEmailDto.receiverEmail = receivEmail.Email;
                receiverEmailDto.Subject = newTransfer.Amount.ToString();
                receiverEmailDto.CUsername = receivEmail.Username;
                receiverEmailDto.Date = emailDate;
                receiverEmailDto.Reference = newTransfer.Reference;
                receiverEmailDto.DebitName = sendEmail.firstName + " " + sendEmail.lastName;
                receiverEmailDto.TransferAmount = newTransfer.Amount.ToString();
                receiverEmailDto.CurrentBalance = data2.Balance.ToString();



                if (newTransfer.Status != "Successful")
                {
                    transfers.message = "Emails not sent because transfer was unsuccessful";
                    transfers.status = false;
                    return transfers;
                }

                await _emailService.creditEmail(receiverEmailDto);
                await _emailService.debitEmail(senderEmailDto);


                var userOutput = await _context.SaveChangesAsync();

                transfers.message = "Transfer Successful";
                transfers.status = true;

                return transfers;
            }
            catch (Exception ex)
            {
                transfers.status = false;
                transfers.message = "Check Backend - Transfer";

                _logger.LogError($"{transfers.message} \n {ex.Message}");

                return transfers;
            }
        }

        public async Task<ActionResult<object>> TransactionVerify(string transfer)
        {    
            //User users = new User();
            LoginView view = new LoginView();
            var info = new RecieverDetails();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new LoginView();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var data = await _context.Accounts.Where(checkId => checkId.userId == userID).FirstAsync();
                if (data == null)
                {
                    view.status = false;
                    view.message = "No one is logged in";

                    return view;
                }

                Transfer newTransfer = new Transfer
                {
                    BeneficiaryAccountNo = transfer
                };

                var userData = await _context.Users
                   .Where(userInfo => userInfo.Account.AccountNo == transfer)
                       .Select(userInfo => new RecieverDetails
                       {
                           ReceiverName = userInfo.firstName + " " + userInfo.lastName,
                           ReceiverAccountNo = transfer
                       })  
                   .FirstOrDefaultAsync();



                if (userData == null)
                {
                    view.status = false;
                    view.message = "This account number cannot be found";

                    return view;
                }

                if (transfer == data.AccountNo)     
                {
                    view.status = false;
                    view.message = "You cannot transfer money to yourself";

                    return view;
                }

                view.data = userData.ReceiverName ;
                view.status = true;
                view.message = "valid account number";

                return view;
            }
            catch (Exception ex)
            {
                view.status = false;
                view.message = "Check Backend - Verify Account Number";

                _logger.LogError($"{view.message} \n {ex.Message}");

                return view;
            }
        }

        public async Task<List<TransferView>> GetTransferHistoryByDate(PdfDto pdfDto)
        {
            TransferView view = new TransferView();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new List<TransferView>();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var transferData = await _context.Transfers
                .Where(transferInfo => transferInfo.DebitUserId == userID || transferInfo.BeneficiaryUserId == userID).Where(x => x.Date >= pdfDto.StartDate && x.Date <= pdfDto.EndDate)
                .Select(transferInfo => new TransferView
                {
                    Date = transferInfo.Date.ToString("MM/dd/yyyy"),
                    BeneficiaryAccountNo = transferInfo.BeneficiaryAccountNo,
                    DUsername = transferInfo.DebitUser.firstName + " " + transferInfo.DebitUser.lastName,
                    CUsername = transferInfo.BeneficiaryUser.firstName + " " + transferInfo.BeneficiaryUser.lastName,
                    Amount = transferInfo.Amount,
                    Status = transferInfo.Status,
                    DebitUserId = transferInfo.DebitUserId,
                    BeneficiaryUserId = transferInfo.BeneficiaryUserId
                })
                .ToListAsync();

                foreach (var transferInfos in transferData)
                {
                    if (transferInfos.DebitUserId == null && transferInfos.BeneficiaryUserId == userID)
                    {
                        transferInfos.TransactionType = "C";
                        transferInfos.DUsername = "Paystack Funding";
                    }
                    if (transferInfos.DebitUserId == userID)
                    {
                        transferInfos.TransactionType = "D";
                    }
                    else
                    {
                        transferInfos.TransactionType = "C";
                    }
                }

                if (transferData == null)
                {
                    return new List<TransferView>();
                }

                return transferData;
            }
            catch (Exception ex)
            {

                _logger.LogError($"Check Backend - Transfer History \n {ex.Message}");
                return new List<TransferView>();
            }
        }
    }
}

//senderEmailDto.senderEmail = sendEmail.Email;
//senderEmailDto.Subject = $"Debit Alert: {newTransfer.Amount}";
//senderEmailDto.Body = $"Dear {sendEmail.Username}, " +
//    $"\n These are the details of the transaction that just occurred on your account <br>" +
//    $"Date/Time: {emailDate}<br>" +
//    $"Reference: {newTransfer.Reference}<br>" +
//    $"Beneficiary: {receiverEmail.firstName}' '{receiverEmail.lastName}<br>" +
//    $"Amount: {newTransfer.Amount}<br>" +
//    $"Current Balance: {data.Balance}<br>" +
//    $"<b>Important Secuirty Information: </b>" +
//    $"If you did not carry out this transaction kindly reach out to our team as soon as possible <br>" +
//    $"Help Line: 09043256734 <br>" +
//    $"Email: customerservice@p2pwallet.com <br>" +
//    $"<br>" +
//    $"<br>" +
//    $"Thank you for choosing P2PWallet";

//receiverEmailDto.receiverEmail = receiverEmail.Email;
//receiverEmailDto.Subject = $"Credit Alert: {newTransfer.Amount}";
//receiverEmailDto.Body = $"Dear {receiverEmail.Username}, " +
//    $"These are the details of the transaction that just occurred on your account" +
//    $"<br>" +
//    $"<br>" +
//    $"Date/Time: {emailDate} <br>" +
//    $"Reference: {newTransfer.Reference} <br>" +
//    $"From: {sendEmail.firstName}' '{sendEmail.lastName} <br>" +
//    $"Amount: {newTransfer.Amount} <br>" +
//    $"Current Balance: {data2.Balance} <br>" +
//    $"<b>Important Secuirty Information: </b> " +
//    $"If you were not the intended receipent kinldy reach out to our team as soon as possible <br>" +
//    $"Help Line: 09043256734 <br>" +
//    $"Email: customerservice@p2pwallet.com <br>" +
//    $"<br>" +
//    $"<br>" +
//    $"Thank you for choosing P2PWallet";