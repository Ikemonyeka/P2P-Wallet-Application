using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using MimeKit;
using MimeKit.Text;
using Newtonsoft.Json;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.AdminDto;
using static P2PWallet.Models.DataObjects.PaystackFundObject;
using static P2PWallet.Models.DataObjects.UserObject;

namespace P2PWallet.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly DataContext _context;
        private readonly LinkGenerator _linkGenerator;

        public EmailService(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, DataContext context, LinkGenerator linkGenerator)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _linkGenerator = linkGenerator;
        }
        public async Task creditEmail(ReceiverEmailDto receiverEmailDto)
        {
            var Template = _configuration.GetSection("EmailDetails:CreditEmailTemplate").Value;

            if (Template == null)
            {

            }

            //var userInfo = await _context.Users.Where(r => r.u).FirstAsync();

            string HtmlBody = "";
            StreamReader reader = new StreamReader(Template);
            HtmlBody = reader.ReadToEnd();
            HtmlBody = HtmlBody.Replace("{Username}", receiverEmailDto.CUsername);
            HtmlBody = HtmlBody.Replace("{TransactionDate}", receiverEmailDto.Date);
            HtmlBody = HtmlBody.Replace("{Reference}", receiverEmailDto.Reference);
            HtmlBody = HtmlBody.Replace("{DebitName}", receiverEmailDto.DebitName);
            HtmlBody = HtmlBody.Replace("{TransferAmount}", receiverEmailDto.TransferAmount);
            HtmlBody = HtmlBody.Replace("{CurrentBalance}", receiverEmailDto.CurrentBalance);
            receiverEmailDto.Body = HtmlBody;


            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailDetails:DefaultEmail").Value));
            email.To.Add(MailboxAddress.Parse(receiverEmailDto.receiverEmail));
            email.Subject = "Credit Alert: " + receiverEmailDto.Subject;
            email.Body = new TextPart("html") { Text = HtmlBody};

            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EmailDetails:EmailHost").Value, 2525, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration.GetSection("EmailDetails:EmailUsername").Value, _configuration.GetSection("EmailDetails:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public async Task debitEmail(SenderEmailDto senderEmailDto)
        {


            var Template = _configuration.GetSection("EmailDetails:DebitEmailTemplate").Value;

            if (Template == null)
            {
              
            }

            //var userInfo = await 

            string HtmlBody = "";
            StreamReader reader = new StreamReader(Template);
            HtmlBody = reader.ReadToEnd();
            HtmlBody = HtmlBody.Replace("{Username}", senderEmailDto.DUsername);
            HtmlBody = HtmlBody.Replace("{TransactionDate}", senderEmailDto.Date);
            HtmlBody = HtmlBody.Replace("{Reference}", senderEmailDto.Reference);
            HtmlBody = HtmlBody.Replace("{CreditName}", senderEmailDto.CreditName);
            HtmlBody = HtmlBody.Replace("{TransferAmount}", senderEmailDto.TransferAmount);
            HtmlBody = HtmlBody.Replace("{CurrentBalance}", senderEmailDto.CurrentBalance);

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailDetails:DefaultEmail").Value));
            email.To.Add(MailboxAddress.Parse(senderEmailDto.senderEmail));
            email.Subject = "Debit Alert: " + senderEmailDto.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = HtmlBody };

            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EmailDetails:EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration.GetSection("EmailDetails:EmailUsername").Value, _configuration.GetSection("EmailDetails:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public async Task<bool> VerificationEmail(VerifyEmailDto verifyEmailDto, string token)
        {

            try
            {
                var Hosturl = _configuration.GetSection("EmailDetails:EmailLocalHost").Value;
                var Curl = _configuration.GetSection("EmailDetails:EmailConfirmation").Value;
                var Template = _configuration.GetSection("EmailDetails:VerificationEmailTemplate").Value;

                if (Template == null)
                {
                    return false;
                }

                string HtmlBody = "";
                StreamReader reader = new StreamReader(Template);
                HtmlBody = reader.ReadToEnd();
                HtmlBody = HtmlBody.Replace("{Link}", string.Format(Hosturl + Curl, verifyEmailDto.Email, verifyEmailDto.Token));

                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailDetails:DefaultEmail").Value));
                email.To.Add(MailboxAddress.Parse(verifyEmailDto.Email));
                email.Subject = "Email Verification";
                email.Body = new TextPart(TextFormat.Html) { Text = HtmlBody };

                using var smtp = new SmtpClient();
                smtp.Connect(_configuration.GetSection("EmailDetails:EmailHost").Value, 587, SecureSocketOptions.StartTls);
                smtp.Authenticate(_configuration.GetSection("EmailDetails:EmailUsername").Value, _configuration.GetSection("EmailDetails:EmailPassword").Value);
                smtp.Send(email);
                smtp.Disconnect(true);

                return true;
            }
            catch 
            {
                return false;
            }
        }

        public async Task<bool> ForgotPasswordEmail(ForgotPasswordDto forgotPasswordDto)
        {

            try
            {
                var Hosturl = _configuration.GetSection("EmailDetails:ForgoPasswordEmailHost").Value;
                var Curl = _configuration.GetSection("EmailDetails:CForgotPassword").Value;
                var Template = _configuration.GetSection("EmailDetails:ForgotPasswordEmail").Value;

                if (Template == null)
                {
                    return false;
                }

                string HtmlBody = "";
                StreamReader reader = new StreamReader(Template);
                HtmlBody = reader.ReadToEnd();
                HtmlBody = HtmlBody.Replace("{Username}", forgotPasswordDto.Username);
                HtmlBody = HtmlBody.Replace("{Link}", string.Format(Hosturl + Curl, forgotPasswordDto.Token));

                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailDetails:DefaultEmail").Value));
                email.To.Add(MailboxAddress.Parse(forgotPasswordDto.Email));
                email.Subject = "Reset Password";
                email.Body = new TextPart(TextFormat.Html) { Text = HtmlBody };

                using var smtp = new SmtpClient();
                smtp.Connect(_configuration.GetSection("EmailDetails:EmailHost").Value, 587, SecureSocketOptions.StartTls);
                smtp.Authenticate(_configuration.GetSection("EmailDetails:EmailUsername").Value, _configuration.GetSection("EmailDetails:EmailPassword").Value);
                smtp.Send(email);
                smtp.Disconnect(true);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task AccountStatement(string userEmail, string username, PdfDate pdfDate, string fileName)
        {
            var Template = _configuration.GetSection("PdfTemplates:PdfEmailTemplate").Value;

            if (Template == null)
            {
                
            }

            //var userInfo = await _context.Users.Where(r => r.u).FirstAsync();

            string HtmlBody = "";
            StreamReader reader = new StreamReader(Template);
            HtmlBody = reader.ReadToEnd();
            HtmlBody = HtmlBody.Replace("{Username}", username);
            HtmlBody = HtmlBody.Replace("{date}", $"{pdfDate.StartDate.ToString("MM/dd/yyyy")} - {pdfDate.EndDate.ToString("MM/dd/yyyy")}");


            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailDetails:DefaultEmail").Value));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = $"STATEMENT OF ACCOUNT {DateTime.Now.ToString("MM/dd/yyyy")}";


            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = HtmlBody;
            bodyBuilder.Attachments.Add(fileName);
            email.Body = bodyBuilder.ToMessageBody();


            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EmailDetails:EmailHost").Value, 2525, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration.GetSection("EmailDetails:EmailUsername").Value, _configuration.GetSection("EmailDetails:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public async Task NewAdminEmail(string username, string password, string userEmail)
        {
            var Template = _configuration.GetSection("EmailDetails:NewAdminTemp").Value;

            if (Template == null)
            {

            }

            string HtmlBody = "";
            StreamReader reader = new StreamReader(Template);
            HtmlBody = reader.ReadToEnd();
            HtmlBody = HtmlBody.Replace("{Username}", username);
            HtmlBody = HtmlBody.Replace("{Password}", password);

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailDetails:DefaultEmail").Value));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = "Account Created";
            email.Body = new TextPart("html") { Text = HtmlBody };

            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EmailDetails:EmailHost").Value, 2525, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration.GetSection("EmailDetails:EmailUsername").Value, _configuration.GetSection("EmailDetails:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);

        }

        public async Task LockedAccountEmail(string description, string userEmail)
        {
            var Template = _configuration.GetSection("EmailDetails:LockedAccountTemp").Value;

            if (Template == null)
            {

            }

            //var userInfo = await 

            string HtmlBody = "";
            StreamReader reader = new StreamReader(Template);
            HtmlBody = reader.ReadToEnd();
            HtmlBody = HtmlBody.Replace("{Description}", description);

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailDetails:DefaultEmail").Value));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = "Account Suspended";
            email.Body = new TextPart(TextFormat.Html) { Text = HtmlBody };

            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EmailDetails:EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration.GetSection("EmailDetails:EmailUsername").Value, _configuration.GetSection("EmailDetails:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public async Task UnlockedEmail(string userEmail)
        {
            var Template = _configuration.GetSection("EmailDetails:UnlockedAccountTemp").Value;

            if (Template == null)
            {

            }

            //var userInfo = await 

            string HtmlBody = "";
            StreamReader reader = new StreamReader(Template);
            HtmlBody = reader.ReadToEnd();

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailDetails:DefaultEmail").Value));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = "Account Suspended";
            email.Body = new TextPart(TextFormat.Html) { Text = HtmlBody };

            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EmailDetails:EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration.GetSection("EmailDetails:EmailUsername").Value, _configuration.GetSection("EmailDetails:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
