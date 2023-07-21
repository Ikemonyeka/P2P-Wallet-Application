using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.UserObject;

namespace P2PWallet.Services.Services
{
    public class PdfService : IPdfServices
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IConverter _convert;
        private readonly IEmailService _emailService;

        public PdfService(DataContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IConverter convert, IEmailService emailService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _convert = convert;
            _emailService = emailService;
        }
        public async Task<byte[]> GetPdf(PdfDto pdfDto)
        {
            LoginView view = new LoginView();
            PdfInfo pdfInfo = new PdfInfo();

            int userID;
            if (_httpContextAccessor.HttpContext == null)
            {
                view.status = false;
                view.message = "Not logged in";

                return null;
            }

            userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

            var x = await _context.Transfers
            .Where(t => t.DebitUserId == userID || t.BeneficiaryUserId == userID).Where(t => t.Date >= pdfDto.StartDate && t.Date <= pdfDto.EndDate)
            .Select(t => new PdfView
            {
                Date = t.Date,
                BeneficiaryAccountNo = t.BeneficiaryAccountNo,
                DUsername = t.DebitUser.firstName + " " + t.DebitUser.lastName,
                CUsername = t.BeneficiaryUser.firstName + " " + t.BeneficiaryUser.lastName,
                Amount = t.Amount,
                DebitUserId = t.DebitUserId,
                BeneficiaryUserId = t.BeneficiaryUserId
            })
            .ToListAsync();

            var userData = await _context.Users
                .Where(y => y.userId == userID).FirstAsync();
            var userData2 = await _context.Accounts
                .Where(y => y.userId == userID).FirstAsync();

            foreach (var i in x)
            {
                if(i.Date >= pdfDto.StartDate && i.Date <= pdfDto.EndDate)
                {
                    if (i.DebitUserId == null && i.BeneficiaryUserId == userID)
                    {
                        i.TransactionType = "Credit";
                        i.DUsername = "Paystack Funding";
                    }
                    if (i.DebitUserId == userID)
                    {
                        i.TransactionType = "Debit";
                    }
                    else
                    {
                        i.TransactionType = "Credit";
                    }
                }
            }

            if(x == null)
            {
                view.status = false;
                view.message = "No Data Found";
                return null;
            }

            PdfInfo info = new PdfInfo
            {
                AccountNo = userData2.AccountNo,
                Currency = userData2.Currency,
                Sender = userData.firstName + " " + userData.lastName,
                Address = userData.Address,
                CBalance = userData2.Balance
            };
                        
            


            var Template = _configuration.GetSection("PdfTemplates:AccStatTemplate").Value;
            if (Template == null)
            {
                view.status = false;
                view.message = "No Data Found";
                return null;
            }
            var HtmlBody = File.ReadAllText(Template);
            StringBuilder reader = new StringBuilder(string.Empty);
            for(int i = 0; i < x.Count; i++)
            {
                reader.Append($"<tr style=\"text-align: center;\">");
                reader.Append($"<td>{x[i].Date.ToString("MM/dd/yyyy")}</td>");
                reader.Append($"<td>{x[i].TransactionType}</td>");
                reader.Append($"<td>{x[i].DUsername}</td>");
                reader.Append($"<td>{x[i].CUsername}</td>");
                reader.Append($"<td>{x[i].Amount}</td>");
                reader.Append($"</tr>");
            }
            HtmlBody = HtmlBody.Replace("{From}", $"{pdfDto.StartDate.ToString("MM/dd/yyyy")} - {pdfDto.EndDate.ToString("MM/dd/yyyy")}");
            HtmlBody = HtmlBody.Replace("{Account}", info.AccountNo);
            HtmlBody = HtmlBody.Replace("{Currency}", info.Currency);
            HtmlBody = HtmlBody.Replace("{Sender}", info.Sender);
            HtmlBody = HtmlBody.Replace("{Address}", info.Address);
            HtmlBody = HtmlBody.Replace("{closingBalance}", info.CBalance.ToString());
            HtmlBody = HtmlBody.Replace("{acc}", reader.ToString());

            PdfDate pdfDate = new PdfDate
            {
                EndDate = pdfDto.EndDate,
                StartDate = pdfDto.StartDate,
            };


         var data =  await PdfGenerator(HtmlBody, pdfDto.File, userData.Username, userData.Email, pdfDate);
            return data;
        }

        public async Task<byte[]> PdfGenerator(string data, string fType, string fname, string userEmail, PdfDate pdfDate)
        {

            if (fType == "downloadPdf")
            {
                string fileName = $"{fname}.pdf";
                var glb = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings()
                    {
                        Bottom = 10,
                        Left = 10,
                        Right = 10,
                        Top = 20
                    },
                    DocumentTitle = $"AccStat{fname}",

                    //Out = Path.Combine(Directory.GetCurrentDirectory(), "Exports", fileName)
                };

                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = data,
                    WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = null }
                };
                var pdf = new HtmlToPdfDocument
                {
                    GlobalSettings = glb,
                    Objects = { objectSettings }
                };

                return _convert.Convert(pdf);

            }
            else if (fType == "emailPdf")
            {
                string fileName = $"{fname}.pdf";
                var glb = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings()
                    {
                        Bottom = 10,
                        Left = 10,
                        Right = 10,
                        Top = 20
                    },
                    DocumentTitle = $"AccStat{fname}",

                    Out = Path.Combine(Directory.GetCurrentDirectory(), "Exports", fileName)
                };

                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = data,
                    WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = null }
                };
                var pdf = new HtmlToPdfDocument
                {
                    GlobalSettings = glb,
                    Objects = { objectSettings }
                };

                 
                var r = _convert.Convert(pdf);
                await _emailService.AccountStatement(userEmail, fname, pdfDate, glb.Out);

                return r;

            }

            return null;


            //string result = $"Files{fileName}";

            //await Task.Yield();

            //string sr = "Pdf generated successfully";

           /* return sr;*/
        }
    }
}
