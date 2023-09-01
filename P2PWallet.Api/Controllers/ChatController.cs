using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Services;
using static P2PWallet.Models.DataObjects.AdminDto;
using static P2PWallet.Models.DataObjects.ChatDto;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : Controller
    {
        private readonly DataContext _context;
        private readonly IChatService _chatService;

        public ChatController(DataContext context, IChatService chatService)
        {
            _context = context;
            _chatService = chatService;
        }

        [HttpPost("ChatUser")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> ChatUser(ChatModel chatModel)
        {
            var result = await _chatService.ChatUser(chatModel);

            return Ok(result);
        }

        [HttpGet("Messages")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> GetUserChat()
        {
            var result = await _chatService.GetUserChat();

            return Ok(result);
        }

        [HttpGet("Unread")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UnreadMessages()
        {
            var result = await _chatService.UnreadMessages();

            return Ok(result);
        }


        [HttpGet("UsersChats")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> UserMessages(string user)
        {
            var result = await _chatService.UserMessages(user);

            return Ok(result);
        }

        [HttpPost("ChatAdmin")]
        [ProducesResponseType(200), Authorize]
        public async Task<IActionResult> ChatAdmin(ChatModelAdmin chatModel)
        {
            var result = await _chatService.ChatAdmin(chatModel);

            return Ok(result);
        }
    }
}
