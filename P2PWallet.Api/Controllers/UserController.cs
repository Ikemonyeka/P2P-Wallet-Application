using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Migrations;
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

        [HttpGet("confirm-email")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<LoginView>> VerifyEmail(string uemail, string utoken)
        {
            var result = await _userService.VerifyEmail(uemail, utoken);

            return Ok(result);
        }

        [HttpPost("ForgotPassword")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<LoginView>> ForgotPassword([FromBody] EmailDto email)
        {
            var result = await _userService.ForgotPassword(email);

            return Ok(result);
        }

        [HttpPost("ResetPassword")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<LoginView>> ResetPassword(ResetPassword resetPassword)
        {
            var result = await _userService.ResetPassword(resetPassword);

            return Ok(result);
        }

        [HttpGet("UserProfile")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<DashboardView>> GetProfile()
        {
            var bio = await _userService.GetProfile();

            return Ok(bio);
        }


        [HttpPut("UpdateProfile")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<LoginView>> UpdateProfile(UpdateUserProfile user)
        {
            var result = await _userService.UpdateProfile(user);

            return Ok(result);
        }

        [HttpPost("ProfilePhoto")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<LoginView>> ProfilePhoto(IFormFile profilePhoto)
        {
            var result = await _userService.ProfilePhoto(profilePhoto);

            return Ok(result);
        }

        [HttpGet("CheckSecurityExists")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<UserView>> CheckSecurityExist()
        {
            var result = await _userService.CheckSecurityExist();

            return Ok(result);
        }

        [HttpPost("CreateSecurityQA")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<UserView>> CreateSecurityQA(SecurityDto securityDto)
        {
            var result = await _userService.CreateSecurityQA(securityDto);

            return Ok(result);
        }

        [HttpPut("UpdatePut")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<LoginView>> UpdatePin(PinDto pinDto)
        {
            var result = await _userService.UpdatePin(pinDto);

            return Ok(result);
        }

        [HttpGet("GetPhoto")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<object>> GetPhoto()
        {
            var img = await _userService.GetPhoto();

            return Ok(img);
        }

        [HttpPost("VerifySecurityAnswer")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<LoginView>> VerifySecurityQA([FromBody] SecurityAnswerDto securityDto)
        {
            var sq = await _userService.VerifySecurityQA(securityDto);

            return Ok(sq);
        }

        [HttpGet("GetQuestion")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<DashboardView>> GetQuestion()
        {
            var question = await _userService.GetQuestion();

            return Ok(question);
        }

        [HttpGet("AvailableWallet")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<List<DCreateNewWallet>>> GetAvailableWallets()
        {
            var sq = await _userService.CreateAvailabeCurrency();

            return Ok(sq);
        }

        [HttpPost("CreateNewWallet")]
        [ProducesResponseType(200), Authorize]
        public async Task<ActionResult<UserView>> CreateNewWallet(string currency)
        {
            var result = await _userService.CreateNewWallet(currency);

            return Ok(result);
        }

        [HttpGet("Currencies")]
        [ProducesResponseType(200), Authorize]
        public async Task<object> GetConversion()
        {
            var result = await _userService.GetConversion();

            return Ok(result);
        }
    }
}


