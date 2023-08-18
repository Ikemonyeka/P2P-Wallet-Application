using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
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
        private readonly INotificationService _notificationService;

        public TransferService(DataContext context, IHttpContextAccessor httpContextAccessor, IEmailService emailService,ILogger<TransferService> logger, INotificationService notificationService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _logger = logger;
            _notificationService = notificationService;
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
                    BeneficiaryUserId = transferInfo.BeneficiaryUserId,
                })
                .ToListAsync();

                foreach(var t in transferData)
                {
                    var x = await _context.Accounts.Where(acc => acc.AccountNo == t.BeneficiaryAccountNo).FirstOrDefaultAsync();
                    t.Currency = x.Currency;
                }

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


                var userData = await _context.Accounts
                .Where(userInfo => userInfo.userId == userID)
                .Select(userInfo => new DashboardView
                {
                    AccountNo = userInfo.AccountNo,
                    Balance = userInfo.Balance,
                    Currency = userInfo.Currency
                })
                .ToListAsync();


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

                //var data = await _context.Accounts.Where(checkId => checkId.Id == userID).FirstAsync();


                var data = await _context.Accounts.Where(i => i.AccountNo == transfer.SourceAccountNo).FirstAsync();
                if (data == null)
                {
                    transfers.message = "Invalid account number";
                    transfers.status = false;
                    return transfers;
                }

                var data2 = await _context.Accounts.Where(i => i.AccountNo == transfer.BeneficiaryAccountNo).FirstAsync();
                if (data2 == null)
                {
                    transfers.message = "Invalid account number";
                    transfers.status = false;
                    return transfers;
                }

                if(data.userId == data2.userId)
                {
                    transfers.message = "You cannot transfer to yourself, go to fund and do that";
                    transfers.status = false;
                    return transfers;
                }

                if (data.Currency != data2.Currency)
                {
                    transfers.message = "You cannot transfer to a different currency";
                    transfers.status = false;
                    return transfers;
                }

                var sendEmail = await _context.Users
                .Where(r => r.userId == userID).FirstAsync();



                var receivEmail = await _context.Accounts
                .Where(r => r.AccountNo == transfer.BeneficiaryAccountNo).FirstAsync();

                var CreditUser = await _context.Users
                .Where(r => r.userId == receivEmail.userId).FirstAsync();

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
                senderEmailDto.CreditName = CreditUser.firstName + " " + CreditUser.lastName;
                senderEmailDto.TransferAmount = newTransfer.Amount.ToString();
                senderEmailDto.CurrentBalance = data.Balance.ToString();


                receiverEmailDto.receiverEmail = receivEmail.User.Email;
                receiverEmailDto.Subject = newTransfer.Amount.ToString();
                receiverEmailDto.CUsername = CreditUser.Username;
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

                await _notificationService.TransferNotification(sendEmail.Username, data.Currency, transfer.Amount, CreditUser.userId);

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

        public async Task<ActionResult<object>> TransactionVerify(TransferVerify transferVerify)
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

                var data = await _context.Accounts.Where(checkId => checkId.AccountNo == transferVerify.SourceAccountNo).FirstAsync();
                if (data == null)
                {
                    view.status = false;
                    view.message = "No one is logged in";

                    return view;
                }

                var data2 = await _context.Accounts.Where(checkId => checkId.AccountNo == transferVerify.BeneficiaryAccountNo).FirstAsync();
                if (data2 == null)
                {
                    view.status = false;
                    view.message = "No one is logged in";

                    return view;
                }

                if (data.userId == data2.userId)
                {
                    view.status = false;
                    view.message = "You cannot transfer money to yourself, you can fund it from the dashboard or the fund E-card tab";

                    return view;
                }

                Transfer newTransfer = new Transfer
                {
                    BeneficiaryAccountNo = transferVerify.BeneficiaryAccountNo
                };

                var userData = await _context.Accounts
                   .Where(userInfo => userInfo.AccountNo == transferVerify.BeneficiaryAccountNo)
                       .Select(userInfo => new RecieverDetails
                       {
                           ReceiverName = userInfo.User.firstName + " " + userInfo.User.lastName,
                           ReceiverAccountNo = transferVerify.BeneficiaryAccountNo
                       })  
                   .FirstOrDefaultAsync();



                if (userData == null)
                {
                    view.status = false;
                    view.message = "This account number cannot be found";

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

        public async Task<UserView> FundForeignCurrrency(TransferSDto transfer)
        {
            UserView view = new UserView();
            int userID;
            if (_httpContextAccessor.HttpContext == null)
            {
                return new UserView();
            }

            userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

            var creditUser = await _context.Accounts.Where(x => x.userId == userID).Where(x => x.AccountNo == transfer.AccountNo).FirstOrDefaultAsync();

            var debitUser = await _context.Accounts.Where(x => x.userId == userID).Where(x => x.Currency == "NGN").FirstOrDefaultAsync();

            var cur = await _context.currencies.Where(x => x.Currency ==creditUser.Currency).FirstAsync();

            if(creditUser == debitUser)
            {
                view.status = false;
                view.message = "Cannot transfer to yourself";

                return view;
            }

            if (creditUser == null)
            {
                view.status = false;
                view.message = "This account number cannot be found";

                return view;
            }
            if (debitUser == null)
            {
                view.status = false;
                view.message = "This account number cannot be found";

                return view;
            }
            if (transfer.Amount <= 0)
            {
                view.status = false;
                view.message = "Cannot transfer zero or a negative value";

                return view;
            }

            var calc = (cur.conversionRate * transfer.Amount);
            if (debitUser.Balance< calc) {
                view.status = false;
                view.message = "You have insufficient funds, please fund your NGN and try again";

                return view;

            }

            var myTransaction = _context.Database.BeginTransaction();

            try
            {
                creditUser.Balance = transfer.Amount + creditUser.Balance;

                debitUser.Balance = debitUser.Balance - calc;

                Transfer newTransfer = new Transfer
                {
                    DebitAccountNo = debitUser.AccountNo,
                    BeneficiaryAccountNo = creditUser.AccountNo,
                    Amount = transfer.Amount,
                    Reference = ReferenceGen(),
                    Status = "Successful",
                    Date = DateTime.UtcNow,
                    DebitUserId = userID,
                    BeneficiaryUserId = creditUser.userId
                };

                await _context.Transfers.AddAsync(newTransfer);
                var userOutput = await _context.SaveChangesAsync();
                myTransaction.Commit();
            }
            catch (Exception ex)
            {
                await myTransaction.RollbackAsync();
            }

            view.status = true;
            view.message = "Transfer Successful";

            return view;
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