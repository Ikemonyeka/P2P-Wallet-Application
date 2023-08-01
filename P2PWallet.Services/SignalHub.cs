using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services
{
    public class SignalHub : Hub
    {
        public async Task SendNotification(string user, string message) 
         {
            await Clients.All.SendAsync("ReceiveNotification", user, message);
        }
    }
}
