using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.DataObjects;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Services;
using static P2PWallet.Models.DataObjects.UserObject;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController : Controller
    {
        private readonly DataContext _context;
        private ITransferService _transferService;
        private readonly IPdfServices _pdfServices;
        private readonly IExcelService _excelService;

        public TransferController(DataContext context, ITransferService transferService, IPdfServices pdfServices, IExcelService excelService)
        {
            _context = context;
            _transferService = transferService;
            _pdfServices = pdfServices;
            _excelService = excelService;
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

        [HttpPost("VerifyBeneficiary")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<object>> TransactionVerify(TransferVerify transferVerify)
        {
            var result = await _transferService.TransactionVerify(transferVerify);

            return Ok(result);
        }

        [HttpPost("PdfGenerator")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> GetPdf(PdfDto pdfDto)
        {
            var result = await _pdfServices.GetPdf(pdfDto);

            return File(result.ToArray(), "application/pdf", "AccStat");
           // var result2 = await _pdfServices.PdfGenerator(result);

            //return result;
        }

        [HttpPost("TransferHistoryByDate")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<TransferView>> GetTransferHistoryByDate(PdfDto pdfDto)
        {
            var transferHistory = await _transferService.GetTransferHistoryByDate(pdfDto);

            return Ok(transferHistory);
        }

        [HttpPost("ExcelGenerator")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> GetExcel(PdfDto pdfDto)
        {
            var result = await _excelService.GetExcel(pdfDto);

            return File(result.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AccStat.xlsx");
            // var result2 = await _pdfServices.PdfGenerator(result);

            //return result;
        }

        [HttpPost("FundForeignCurrrency")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> FundForeignCurrrency(TransferSDto transfer)
        {
            var result = await _transferService.FundForeignCurrrency(transfer);

            return Ok(result);
        }
    }
}
