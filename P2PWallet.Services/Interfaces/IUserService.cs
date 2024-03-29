﻿using Microsoft.AspNetCore.Http;
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
        Task<object> GetProfile();
        Task<object> UpdateProfile(UpdateUserProfile user);
        Task<object> ProfilePhoto(IFormFile profilePhoto);
        Task<ActionResult<UserView>> CheckSecurityExist();
        Task<UserView> CreateSecurityQA(SecurityDto securityDto);
        Task<UserView> VerifySecurityQA(SecurityAnswerDto securityDto);
        Task<object> UpdatePin(PinDto pinDto);
        Task<object> GetPhoto();
        Task<object> GetQuestion();
        Task<object> CreateAvailabeCurrency();
        Task<UserView> CreateNewWallet(string currency);
        Task<object> GetConversion();
        Task<object> CurrentUserForChat();
        Task<object> IsKYCVerified();
        Task<object> CompleteKYCUploadedCheck();
    }
}
