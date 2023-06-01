using Microsoft.EntityFrameworkCore;
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
        Task<ActionResult<LoginView>> VerifyEmail(string uemail, string utoken);
        Task<ActionResult<LoginView>> ForgotPassword(EmailDto email);
        Task<ActionResult<LoginView>> ResetPassword(ResetPassword resetPassword);
        //Task<object> GetProfile();
    }
}


//public async Task<LoginView> GetVToken()
//{
//    User user = new User();
//    LoginView view = new LoginView();
//    VerifyEmailDto verifyEmailDto = new VerifyEmailDto();

//    var data = await _context.Users.Where(tokenCheck => tokenCheck.Email == verifyEmailDto.Email)
//        .Select(tokenCheck => new VerifyEmailDto
//        {
//            Token = tokenCheck.VerificationToken
//        })
//        .FirstAsync();

//    if (data == null)
//    {
//        view.status = false;
//        view.message = "Registration/Token Error";
//        return view;
//    }

//    view.status = true;
//    view.message = data.ToString();

//    return view;

//}