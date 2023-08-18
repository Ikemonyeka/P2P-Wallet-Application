using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly DataContext _context;
        private readonly IHubContext<SignalHub> _hub;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(DataContext context, IHubContext<SignalHub> hub, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _hub = hub;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<object> GetNotificationsDashboard()
        {
            int userID;
            if (_httpContextAccessor.HttpContext == null)
            {
                return new DashboardView();
            }

            userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);


            var notifications = await _context.Notifications.Where(x => x.userId == userID).OrderByDescending(x => x.Date).Take(3).ToListAsync();

            var arrNotes = notifications.ToArray();

            return arrNotes;
        }

        public async Task<object> GetAllNotificationsDashboard()
        {
            int userID;
            if (_httpContextAccessor.HttpContext == null)
            {
                return new DashboardView();
            }

            userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);


            var notifications = await _context.Notifications.Where(x => x.userId == userID).ToListAsync();

            return notifications;
        }
        public async Task TransferNotification(string sender, string currency, decimal amount, int recerverId)
        {
            try
            {
                //var x = await _context.Notifications.Where(i => i.userId == recerverId).FirstOrDefaultAsync();
                var user = await _context.Users.Where(i => i.userId == recerverId).FirstOrDefaultAsync();

                Notifications notification = new Notifications
                {
                    Date = DateTime.Now,
                    Title = $"Credit Alert: {amount}.",
                    Message = $"Hi {user.Username}, you have been credited with {currency}{amount} from {sender}",
                    Status = false,
                    userId = recerverId
                };

                await _context.Notifications.AddAsync(notification);

                await _context.SaveChangesAsync();

                await _hub.Clients.All.SendAsync("ReceiveCreditAlert", user.Username, notification.Message);

                return;

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
