using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Web.WebPages;
using static P2PWallet.Models.DataObjects.PaystackFundObject;

namespace P2PWallet.Services.Services
{
    public class PaystackFundService : IPaystackFundService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaystackFundService> _logger;
        HttpClient client = new HttpClient();
        HttpRequestMessage request;
        HttpResponseMessage response;

        public PaystackFundService(DataContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<PaystackFundService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
            _logger.LogDebug("NLog injected into PaystackFundService");
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
        public async Task<ActionResult<object>> InitializePaystack(decimal fundPaystack)
        {
            PaystackFundView paystackFundView = new PaystackFundView();
            PaystackFundDto paystackFundDto = new PaystackFundDto();

            var paystackUrl = System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("Paystack:InitializeUrl").Value);
            string urlString = Encoding.UTF8.GetString(paystackUrl);
            var key = System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("PayStackKeys:Secret_Key").Value);
            string keyString = Encoding.UTF8.GetString(key);


            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return paystackFundDto;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var userData = await _context.Users
                .Where(emailInfo => emailInfo.userId == userID).FirstAsync();


                if (userData == null)
                {
                    paystackFundDto.status = false;
                    paystackFundDto.message = "No email found";
                    return paystackFundDto;
                }

                var rawAmount = fundPaystack * 100;
                var strAmount = rawAmount.ToString();
                var reference = ReferenceGen();

                paystackFundView.amount = strAmount;
                paystackFundView.email = userData.Email;
                paystackFundView.reference = reference;


                client = new HttpClient();
                request = new HttpRequestMessage(HttpMethod.Post, urlString);

                var stringData = JsonConvert.SerializeObject(paystackFundView);
                var stringContent = new StringContent(stringData, Encoding.UTF8, "application/json");
                
                request.Content = stringContent;

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", keyString);

                response = await client.SendAsync(request);

                string responsebody = await response.Content.ReadAsStringAsync();

                var responseData = JsonConvert.DeserializeObject<PaystackFundModel>(responsebody);

                PaystackFund paystackFund = new PaystackFund
                {
                    Amount = fundPaystack,
                    Reference = responseData.data.reference,
                    userId = userID
                };

                await _context.PaystackFunds.AddAsync(paystackFund);

                var paySave = await _context.SaveChangesAsync();

                paystackFundDto.status = responseData.status;
                paystackFundDto.data = responseData.data.authorization_url;

                return paystackFundDto;

            }
            catch (Exception ex)
            {
                paystackFundDto.status = false;
                paystackFundDto.message = "Check Backend - Initialize Paystack Failed";

                _logger.LogError($"{paystackFundDto.message} \n {ex.Message}");

                return paystackFundDto;
            }
        }

        public async Task<ActionResult> Webhooks(object obj)
        {
            PaystackFund paystackFund = new PaystackFund();
            LoginView view = new LoginView();

            try
            {
                var jsonData = JsonConvert.DeserializeObject<WebhookDTO>(obj.ToString());

                var amount = jsonData.data.amount / 100;
                var email = jsonData.data.customer.email;
                var reference = jsonData.data.reference;
                //var status;

                var paystackData = await _context.PaystackFunds
                .Where(r => r.Amount == amount && r.Reference == reference && r.Status == "Pending").FirstAsync();

                if(paystackData == null)
                {
                    view.status = false;
                    view.message = "Details do not match with database";
                }

                var data2 = await _context.Accounts.Where(checkAcc => checkAcc.userId == paystackData.userId).FirstAsync();
                //&& checkAcc.Currency == paystackData.Currency
                if (data2 == null)
                {
                    return null;
                }

                data2.Balance =  data2.Balance + paystackData.Amount;
                //var bal = paystackData.User.Account.Balance + paystackFund.Amount;


                paystackData.Status = "Successful";

                await _context.SaveChangesAsync();

                Transfer transfer = new Transfer
                {
                    BeneficiaryAccountNo = data2.AccountNo,
                    Amount = paystackData.Amount,
                    Reference = paystackData.Reference,
                    Status = paystackData.Status,
                    Date = DateTime.Now,
                    BeneficiaryUserId = paystackData.userId
                };

                await _context.Transfers.AddAsync(transfer);

                await _context.SaveChangesAsync();

                TransferView transferView = new TransferView
                {
                    //just added line 200
                    BeneficiaryUserId = paystackData.userId,
                    DUsername = "Paystack Funding"
                };

                
                return null;
            }
            catch (Exception ex)
            {
                view.message = "Check Backend - Webhook Payment Unsuccessful";
                view.status = false;

                _logger.LogError($"{view.message} \n {ex.Message}");

                return null;
            }
        }
    }
}
