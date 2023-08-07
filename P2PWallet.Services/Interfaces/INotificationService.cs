using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.UserObject;

namespace P2PWallet.Services.Interfaces
{
    public interface INotificationService
    {
        Task TransferNotification(string sender, string currency, decimal amount, int recerverId);
        Task<object> GetNotificationsDashboard();
        Task<object> GetAllNotificationsDashboard();
    }
}
