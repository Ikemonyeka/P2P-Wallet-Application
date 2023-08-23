using P2PWallet.Models.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.AdminDto;

namespace P2PWallet.Services.Interfaces
{
    public interface IAdminService
    {
        Task<ResponseObject> CreateNewAdmin(AdminR admin);
        Task<ResponseMessageModel<bool>> LoginAdmin(AdminLogin login);
        Task<object> GetAdmin();
        Task<object> GetCurrencyRate();
        Task<object> GetSummaryTransfersAdmin();
        Task<object> GetUsers();
        Task<ResponseMessageModel<bool>> EnableDisableProfile(profileStatus profileStatus);
        Task<object> GetDescriptions();
        Task<ResponseMessageModel<bool>> DescriptionOfLU(DescriptionLU descriptionLU);
        Task<ResponseMessageModel<bool>> SetAdminPassword(AdminPassword adminpassword);
        Task<object> FindUser(string identifier);
    }
}
