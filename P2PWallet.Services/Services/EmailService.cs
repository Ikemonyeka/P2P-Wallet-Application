using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using MimeKit;
using MimeKit.Text;
using P2PWallet.Models.DataObjects;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly DataContext _context;

        public EmailService(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, DataContext context)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }
        public async Task creditEmail(ReceiverEmailDto receiverEmailDto)
        {
            var CreditPathToFile = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() + "Templates" +
                 Path.DirectorySeparatorChar.ToString() + "EmailTemplate" + Path.DirectorySeparatorChar.ToString()
                 + "ReceiverTransferEmailTemplate.html";

            //var userInfo = await _context.Users.Where(r => r.u).FirstAsync();

            string HtmlBody = "";
            StreamReader reader = new StreamReader("C:\\Users\\ikemo\\source\\repos\\P2PWallet\\P2PWallet.Api\\wwwroot\\Templates\\EmailTemplate\\ReceiverTransferEmailTemplate.html");
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


            var DebitPathToFile = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() + "Templates" +
            Path.DirectorySeparatorChar.ToString() + "EmailTemplate" + Path.DirectorySeparatorChar.ToString()
            + "SenderTransferEmailTemplate.html";

            //var userInfo = await 

            string HtmlBody = "";
            StreamReader reader = new StreamReader("C:\\Users\\ikemo\\source\\repos\\P2PWallet\\P2PWallet.Api\\wwwroot\\Templates\\EmailTemplate\\SenderTransferEmailTemplate.html");
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
    }
}
