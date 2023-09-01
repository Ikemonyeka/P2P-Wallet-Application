using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.AdminDto;
using static P2PWallet.Models.DataObjects.ChatDto;

namespace P2PWallet.Services.Interfaces
{
    public interface IChatService
    {
        Task<ResponseMessageModel<bool>> ChatUser(ChatModel chatModel);
        Task<ResponseMessageModel<bool>> ChatAdmin(ChatModelAdmin chatModel);
        Task<object> GetUserChat();
        Task<object> UnreadMessages();
        Task<object> UserMessages(string user);
    }
}
