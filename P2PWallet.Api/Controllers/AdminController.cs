using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Services;
using static P2PWallet.Models.DataObjects.AdminDto;
using static P2PWallet.Models.DataObjects.UserObject;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController: Controller
    {
        private readonly DataContext _context;
        private readonly IAdminService _adminService;

        public AdminController(DataContext context, IAdminService adminService)
        {
            _context = context;
            _adminService = adminService;
        }

        [HttpPost("Register")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CreateNewAdmin(AdminR admin)
        {
            var result = await _adminService.CreateNewAdmin(admin);

            return Ok(result);
        }

        [HttpPost("Login")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> LoginAdmin(AdminLogin login)
        {
            var result = await _adminService.LoginAdmin(login);

            return Ok(result);
        }

        [HttpGet("GetAdmin")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> GetAdmin()
        {
            var result = await _adminService.GetAdmin();

            return Ok(result);
        }

        [HttpGet("Currencies")]
        [ProducesResponseType(200), Authorize]
        public async Task<object> GetCurrencyRate()
        {
            var result = await _adminService.GetCurrencyRate();

            return Ok(result);
        }

        [HttpGet("SummaryOfTransfers")]
        [ProducesResponseType(200), Authorize]
        public async Task<object> GetSummaryTransfersAdmin()
        {
            var result = await _adminService.GetSummaryTransfersAdmin();

            return Ok(result);
        }

        [HttpGet("AllUsers")]
        [ProducesResponseType(200)]
        public async Task<object> GetUsers()
        {
            var result = await _adminService.GetUsers();

            return Ok(result);
        }

        [HttpPost("ProfileStatus")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> EnableDisableProfile(profileStatus profileStatus)
        {
            var result = await _adminService.EnableDisableProfile(profileStatus);

            return Ok(result);
        }

        [HttpGet("Descriptions")]
        [ProducesResponseType(200), Authorize]
        public async Task<object> GetDescriptions()
        {
            var result = await _adminService.GetDescriptions();

            return Ok(result);
        }

        [HttpPost("LockedOrUnlockedDesc")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> DescriptionOfLU(DescriptionLU descriptionLU)
        {
            var result = await _adminService.DescriptionOfLU(descriptionLU);

            return Ok(result);
        }
    }
}
