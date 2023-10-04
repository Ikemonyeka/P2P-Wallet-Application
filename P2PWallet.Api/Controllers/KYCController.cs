using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Services;
using static P2PWallet.Models.DataObjects.ChatDto;
using static P2PWallet.Models.DataObjects.KYCDto;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KYCController : Controller
    {
        private readonly DataContext _context;
        private readonly IKYCService _kYCService;

        public KYCController(DataContext context, IKYCService kYCService)
        {
            _context = context;
            _kYCService = kYCService;
        }

        [HttpPost("CreateNewKYCField")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> ChatUser(NewKYCField newKYC)
        {
            var result = await _kYCService.CreateNewKYCField(newKYC);

            return Ok(result);
        }

        [HttpGet("GetKYCReq")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> GetKYCRequirements()
        {
            var result = await _kYCService.GetKYCRequirements();

            return Ok(result);
        }

        [HttpPost("NewKYCUpload")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> NewKYCUpload([FromForm] newUpload upload)
        {
            var result = await _kYCService.NewKYCUpload(upload);

            return Ok(result);
        }

        [HttpGet("GetListKYC")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> GetListKYC()
        {
            var result = await _kYCService.GetListKYC();
            
            return Ok(result);
        }

        [HttpPost("RemoveKYCReq")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> RemoveKYCReq(string code)
        {
            var result = await _kYCService.RemoveKYCReq(code);

            return Ok(result);
        }

        [HttpGet("KYCPending")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> KYCPending(int page)
        {
            var result = await _kYCService.KYCPending(page);

            return Ok(result);
        }

        [HttpPost("AcceptUpload")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> AcceptUpload(AcceptUserUpload upload)
        {
            var result = await _kYCService.AcceptUpload(upload);

            return Ok(result);
        }

        [HttpPost("RejectUpload")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> RejectUpload(RejectUserUpload upload)
        {
            var result = await _kYCService.RejectUpload(upload);

            return Ok(result);
        }
    }
}
