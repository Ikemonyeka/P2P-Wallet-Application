using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.UserObject;

namespace P2PWallet.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserView> RegisterUser(UserDto user);
        Task<LoginView> LoginUser(LoginDto user);
        Task<object> GetDashboard();
        Task<UserView> CheckPin();
        Task<UserView> CreatePin(PinDto pinDto);
        Task<UserView> VerifyPin(PinDto pinDto);
    }
}
