using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using static P2PWallet.Models.DataObjects.UserObject;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly DataContext _context;
        private IUserService _userService;

        public UserController(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpPost("Register")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<UserView>> RegisterUser([FromBody] UserDto user)
        {
            var result = await _userService.RegisterUser(user);

            return Ok(result);
        }

        [HttpPost("Login")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<LoginView>> LoginUser([FromBody] LoginDto user)
        {
            var result = await _userService.LoginUser(user);

            return Ok(result);
        }


        [HttpGet("Dashboard")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<DashboardView>> GetDashboard()
        {
            var dashboard = await _userService.GetDashboard();

            return Ok(dashboard);
        }

        [HttpGet("CheckPinExists")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<UserView>> CheckPin()
        {
            var result = await _userService.CheckPin();

            return Ok(result);
        }


        [HttpPost("CreataePin")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<UserView>> CreatePin([FromBody] PinDto pinDto)
        {
            var result = await _userService.CreatePin(pinDto);

            return Ok(result);
        }

        [HttpPost("VerifyPin")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<LoginView>> VerifyPin([FromBody] PinDto pinDto)
        {
            var result = await _userService.VerifyPin(pinDto);

            return Ok(result);
        }
    }
}
