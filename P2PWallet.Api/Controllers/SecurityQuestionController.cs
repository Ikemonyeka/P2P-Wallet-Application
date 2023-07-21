using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using static P2PWallet.Models.DataObjects.UserObject;

namespace P2PWallet.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SecurityQuestionController : Controller
    {
        private readonly DataContext _context;
        private ISeedSecurityQuestion _securityQuestion;

        public SecurityQuestionController(DataContext context, ISeedSecurityQuestion securityQuestion)
        {
            _context = context;
            _securityQuestion = securityQuestion;
        }

        [HttpGet("SecurityQuestions")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<List<SeededQ>>> GetQuestions()
        {
            var sq = await _securityQuestion.GetQuestions();

            return Ok(sq);
        }
    }
}
