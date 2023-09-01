using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.AdminDto;
using static P2PWallet.Models.DataObjects.ChatDto;

namespace P2PWallet.Services.Services
{
    public class ChatService : IChatService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<SignalHub> _hub;

        public ChatService(DataContext context, IHttpContextAccessor httpContextAccessor, IHubContext<SignalHub> hub)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _hub = hub;
        }

        public async Task<ResponseMessageModel<bool>> ChatAdmin(ChatModelAdmin chatModel)
        {
            int adminID;
            if (_httpContextAccessor.HttpContext == null)
            {
                return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };
            }

            adminID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

            var user = await _context.Users.Where(x => x.Username == chatModel.username).FirstOrDefaultAsync();
            if (user == null) return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };

            Chat chatSave = new Chat
            {
                message = chatModel.message,
                date = DateTime.Now,
                messageType = "Admin Sent",
                isReadUser = false,
                isReadAdmin = true,
                userId = user.userId,
                adminId = adminID,
            };

            await _context.AddAsync(chatSave);
            await _context.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("ChatUser", chatModel.username, chatSave.message);

            return new ResponseMessageModel<bool> { status = true, message = "saved and sent", data = true };
        }

        public async Task<ResponseMessageModel<bool>> ChatUser(ChatModel chatModel)
        {
            int userID;
            if (_httpContextAccessor.HttpContext == null)
            {
                return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };
            }

            userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

            var user = await _context.Users.Where(x => x.userId == userID).FirstOrDefaultAsync();
            if (user == null) return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };

            Chat chatSave = new Chat
            {
                message = chatModel.message,
                date = DateTime.Now,
                messageType = "User Sent",
                isReadUser = true,
                isReadAdmin = false,
                userId = userID,
            };

            await _context.AddAsync(chatSave);
            await _context.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("ChatUser", user.Username, chatSave.message);

            return new ResponseMessageModel<bool> { status = true, message = "saved and sent", data = true };
        }

        

        public async Task<object> GetUserChat()
        {
            int userID;
            if (_httpContextAccessor.HttpContext == null)
            {
                return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };
            }

            userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

            var chat = await _context.Chat.Where(x => x.userId == userID).Select(x => new ChatMessage
            {
                message = x.message,
                date = x.date.ToString("hh:mm tt"),
                userId = x.userId,
                messageType = x.messageType

            }).ToListAsync();

            var chatStore = chat.ToArray();

            return chat;
        }

        public async Task<object> UnreadMessages()
        {
            var unread = await _context.Chat.Where(x => x.isReadAdmin == false).
                Select(x => new UnreadMessage
            {
                message = x.message,
                date = x.date.ToString("MM/dd/yyyy"),
                userId = x.userId,
                messageType = x.messageType,
                username = x.User.Username

            }).GroupBy(x => x.username).ToDictionaryAsync(group => group.Key, group => group.ToList());

            var unreadArr = unread.ToArray();

            return unreadArr;
        }

        public async Task<object> UserMessages(string user)
        {
            try
            {
                List<ChatsDto> chatsDto = new List<ChatsDto>();
                int userID;
                if (_httpContextAccessor.HttpContext == null) return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var userCheck = await _context.Users.Where(x => x.Username == user).FirstOrDefaultAsync();

                if (userCheck == null) return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };

                var messages = await _context.Chat.Where(x => x.userId == userCheck.userId).ToListAsync();
                if (messages.Count <= 0)
                {
                    return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };
                }

                foreach (var message in messages)
                {
                  message.isReadAdmin = true;
                  message.adminId = message.adminId == null ? userID :message.adminId;
                }

                await _context.SaveChangesAsync();

                foreach (var message in messages)
                {
                    chatsDto.Add(new ChatsDto { 
                        message = message.message,
                        date = message.date.ToString("MM/dd/yyyy"),
                        messageType = message.messageType,
                        isReadAdmin = message.isReadAdmin
                    });
                }

                return chatsDto;

            }
            catch
            {
                return new ResponseMessageModel<bool> { status = false, message = "system back check", data = false };
            }
        }
    }
}
