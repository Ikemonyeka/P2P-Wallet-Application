using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Services;
using static P2PWallet.Models.DataObjects.AdminDto;
using static P2PWallet.Models.DataObjects.GLDto;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GLController : Controller
    {
        private readonly DataContext _context;
        private readonly IGLService _gLService;

        public GLController(DataContext context, IGLService gLService)
        {
            _context = context;
            _gLService = gLService;
        }

        [HttpPost("CreateGL")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> CreateGL(CreateGL createGL)
        {
            var result = await _gLService.CreateGL(createGL);

            return Ok(result);
        }
    }
}
