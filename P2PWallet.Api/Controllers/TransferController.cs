using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.DataObjects;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Services;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController : Controller
    {
        private readonly DataContext _context;
        private ITransferService _transferService;
        public TransferController(DataContext context, ITransferService transferService)
        {
            _context = context;
            _transferService = transferService;
        }

        [HttpPost("Transaction")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<TransferView>> Transaction([FromBody] TransferDto transfer)
        {
            var result = await _transferService.Transaction(transfer);

            return Ok(result);
        }

        [HttpGet("DebitUserInfo")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<DashboardView>> GetDebitUser()
        {
            var debitUser = await _transferService.GetDebitUser();

            return Ok(debitUser);
        }

        [HttpGet("TransferHistory")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<TransferView>> GetTransferHistory()
        {
            var transferHistory = await _transferService.GetTransferHistory();

            return Ok(transferHistory);
        }

        [HttpPost]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<object>> TransactionVerify(string transfer)
        {
            var result = await _transferService.TransactionVerify(transfer);

            return Ok(result);
        }
    }
}
