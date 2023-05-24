using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using P2PWallet.Models.DataObjects;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Services;
using System.Net;
using System.Security.Cryptography;
using System.Text;
//using System.Web.Mvc;
using static P2PWallet.Models.DataObjects.PaystackFundObject;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaystackController : Controller
    {
        private readonly DataContext _context;
        private readonly IPaystackFundService _paystackFundService;
        private readonly IConfiguration _configuration;

        public PaystackController(DataContext context, IPaystackFundService paystackFundService, IConfiguration configuration)
        {
            _context = context;
            _paystackFundService = paystackFundService;
            _configuration = configuration;
        }

        [HttpPost("Initialization")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<object>> InitializePaystack(decimal fundPaystack)
        {
            var result = await _paystackFundService.InitializePaystack(fundPaystack);

            return Ok(result);
        }

        [HttpPost("Webhooks")]
        [ProducesResponseType(200)]
        public async Task<ActionResult> Webhooks(object obj)
        {
            var key = System.Text.Encoding.UTF8.GetBytes(
             _configuration.GetSection("PayStackKeys:Secret_Key").Value);
            string keyString = Encoding.UTF8.GetString(key);

            var reqHeader = HttpContext.Request.Headers;
            //var jsonInput = new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            //String inputString = Convert.ToString(new JValue(jsonInput));
            String result = "";

            byte[] secretkeyBytes = Encoding.UTF8.GetBytes(keyString);
            byte[] inputBytes = Encoding.UTF8.GetBytes(obj.ToString());
            using (var hmac = new HMACSHA512(secretkeyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                result = BitConverter.ToString(hashValue).Replace("-", string.Empty); ;
            }

            reqHeader.TryGetValue("x-paystack-signature", out StringValues xpaystackSignature);

            if (!result.ToLower().Equals(xpaystackSignature))
            {
                return BadRequest();
            }
            await _paystackFundService.Webhooks(obj);

            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

            return Ok();

        }
    }
}
