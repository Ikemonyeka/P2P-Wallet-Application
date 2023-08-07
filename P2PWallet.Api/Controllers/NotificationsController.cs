using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.DataObjects;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Services;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController: Controller
    {
        private readonly DataContext _context;
        private readonly INotificationService _notificationService;

        public NotificationsController(DataContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet("Dashboard")]
        [ProducesResponseType(200), Authorize]
        public async Task<object> GetNotificationsDashboard()
        {
            var notifications = await _notificationService.GetNotificationsDashboard();

            return Ok(notifications);
        }

        [HttpGet("AllNotifications")]
        [ProducesResponseType(200), Authorize]
        public async Task<object> GetAllNotificationsDashboard()
        {
            var notifications = await _notificationService.GetAllNotificationsDashboard();

            return Ok(notifications);
        }
    }
}
