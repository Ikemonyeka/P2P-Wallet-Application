using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using NPOI.SS.Formula.Functions;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using static P2PWallet.Models.DataObjects.AdminDto;
using static P2PWallet.Models.DataObjects.PaystackFundObject;
using static P2PWallet.Models.DataObjects.UserObject;



namespace P2PWallet.Services.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;

        public UserService(DataContext context, IConfiguration configuration, IEmailService emailService, IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "NLog injected into UserService");
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private void CreatePinHash(string pin, out byte[] pinHash, out byte[] pinSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                pinSalt = hmac.Key;
                pinHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pin));
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                 new Claim(ClaimTypes.SerialNumber, user.userId.ToString("")),
                new Claim(ClaimTypes.Name, user.Username),
               
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(60 ),
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;

        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private bool VerifyPinHash(string pin, byte[] pinHash, byte[] pinSalt)
        {
            using (var hmac = new HMACSHA512(pinSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pin));
                return computedHash.SequenceEqual(pinHash);
            }
        }

        private static string AccountGen()
        {
            var now = DateTime.Now;
            var random = $"{now.ToString("MMffdd")}{new Random().Next(1111,9999)}";
            var AcctNumber = random;
            return AcctNumber.ToString();
        }

        private static string VerficationGen()
        {
            const string src = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int length = 40;
            var sb = new StringBuilder();
            Random RNG = new Random();
            for (var i = 0; i < length; i++)
            {
                var c = src[RNG.Next(0, src.Length)];
                sb.Append(c);
            }
            return sb.ToString();
        }

        public async Task<UserView> RegisterUser(UserDto user)
        {
            UserView users = new UserView();
            VerifyEmailDto verifyEmailDto = new VerifyEmailDto();

            try
            {
                CreatePasswordHash(user.Password, out byte[] passwordHash, out byte[] passwordSalt);
                 
                if (!user.Password.Equals(user.cPassword))
                {
                    users.message = "Passwords do not match";
                    return users;
                }

                var data = await _context.Users.AnyAsync(userCheck => userCheck.Username == user.Username || userCheck.Email == user.Email);
                if (data)
                {
                    users.message = "Email or Username exist";
                    return users;
                }

                User userInfo = new User
                {
                    Username = user.Username,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    firstName = user.firstName,
                    lastName = user.lastName,
                    Address = user.Address,
                    VerificationToken = VerficationGen(),
                };

                await _context.Users.AddAsync(userInfo);

                var userOutput = await _context.SaveChangesAsync();

                var vEmail = await _context.Users
                .Where(r => r.Email == userInfo.Email).FirstAsync();

                if (vEmail == null)
                {
                    users.status = false;
                    users.data = "";
                    users.message = "System Error";
                }


                verifyEmailDto.Email = vEmail.Email;
                verifyEmailDto.Token = vEmail.VerificationToken;

                Account newAccount = new Account
                {
                    AccountNo = AccountGen(),
                    Balance = 10000,
                    Currency = "NGN",
                    userId = userInfo.userId
                };

                await _context.Accounts.AddAsync(newAccount);

                var accountOutput = await _context.SaveChangesAsync();

                SecurityQuestion securityQuestion = new SecurityQuestion
                {
                    userId = userInfo.userId
                };

                await _context.securityQuestions.AddAsync(securityQuestion);

                await _context.SaveChangesAsync();


                if(verifyEmailDto.Token == null)
                {
                    users.message = "Registration Unsuccessful";
                    users.status = false;

                    return users;
                }

                await _emailService.VerificationEmail(verifyEmailDto, verifyEmailDto.Token);
 
                users.message = "Registration Successful, Please check your email to verify your account";
                users.data = userInfo.VerificationToken;
                users.status = true;

                return users;
            }
            catch(Exception ex)
            {
                users.message = "Check Backend - Registration Unsuccessful";
                users.status = false;

                _logger.LogError($"{users.message} \n {ex.Message}");

                return users;
            }
        }

        public async Task<LoginView> LoginUser(LoginDto user)
        {
            User users = new User();
            LoginView view = new LoginView();
            try
            {
                var data = await _context.Users.Where(userCheck => userCheck.Username == user.Username).FirstOrDefaultAsync();
                if (data == null)
                {
                    view.status = false;
                    view.message = "Incorrect Username or Password";
                    return view;
                }

                if(data.Status == false)
                {
                    view.status = false;
                    view.message = "Account Is Locked, Reach out to customer care f this is strange";
                    return view;
                }

                if (data.VerifiedAt == null)
                {
                    view.status = false;
                    view.message = "You have not verified your account yet, please do so to Login on P2PWallet";
                    return view;
                }

                if (!VerifyPasswordHash(user.Password, data.PasswordHash, data.PasswordSalt))
                {
                    view.status = false;
                    view.message = "Incorrect Username or Password";
                    return view;
                }


                users.userId = data.userId;
                users.Username = data.Username;
                var token = CreateToken(users);
                view.status = true;
                view.message = "Login Succesful";
                view.data = token;

                return view;
            }
            catch (Exception ex)
            {
                view.message = "Check Backend - Login Unsuccessful";
                view.status = false;

                _logger.LogError($"{view.message} \n {ex.Message}");

                return view;
            }
        }

        public async Task<object> GetDashboard()
        {
            UserView users = new UserView();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new DashboardView();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);


                var userData = await _context.Accounts
                .Where(userInfo => userInfo.userId == userID)
                .Select(userInfo => new AvailableCurrency
                {
                    Username = userInfo.User.Username,
                    AccountNo = userInfo.AccountNo,
                    Balance = userInfo.Balance,
                    Currency = userInfo.Currency
                })
                .ToListAsync();

                var change = await _context.Users.Where(x => x.userId == 2).FirstOrDefaultAsync();

                change.VerifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();


                foreach (var x in userData)
                {
                    if(x.Currency == "NGN")
                    {
                        x.Fund = false;
                    }
                    else
                    {
                        x.Fund = true;
                    }
                }



                if (userData == null) return new DashboardView();


                return userData;
            }
            catch (Exception ex)
            {
                users.message = "Check Backend - Dashboard Display";
                users.status = false;

                _logger.LogError($"{users.message} \n {ex.Message}");

                return users;
            }
        }

        public async Task<UserView> CheckPin()
        {
            UserView users = new UserView();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return null;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var userData = await _context.Users
                .Where(pinInfo => pinInfo.userId == userID).FirstAsync();


                var strPinHash = userData.PinHash;
                var strPinSalt = userData.PinSalt;


                if (strPinHash == null || strPinSalt == null)
                {
                    users.status = false;
                    users.message = "Please create a pin";
                    return users;
                }

                users.message = "Pin Exists";
                users.status = true;

                return users;
            }
            catch (Exception ex)
            {
                users.message = "Check Backend - Check Pin";
                users.status = false;

                _logger.LogError($"{users.message} \n {ex.Message}");

                return users;
            }
        }

        public async Task<UserView> CreatePin(PinDto pinDto)
        {
            UserView users = new UserView();

            CreatePinHash(pinDto.Pin, out byte[] pinHash, out byte[] pinSalt);
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return null;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var userData = await _context.Users
                .Where(pinInfo => pinInfo.userId == userID).FirstAsync();

                if (pinDto.Pin.Length != 4)
                {
                    users.status = false;
                    users.message = "Your pin must be 4 digits";
                    return users;
                }

                userData.PinHash = pinHash;
                userData.PinSalt = pinSalt;

                var strPinHash = userData.PinHash.Length;
                var strPinSalt = userData.PinSalt.Length;


                if (strPinHash <= 5 || strPinSalt <= 5)
                {
                    users.status = false;
                    users.message = "Please create a pin";
                    return users;
                }

                await _context.SaveChangesAsync();

                users.status = true;
                users.message = "pin created";
                return users;
            }
            catch (Exception ex)
            {
                users.message = "Check Backend - Create Pin Unsuccessful";
                users.status = false;

                _logger.LogError($"{users.message} \n {ex.Message}");

                return users;
            }

        }

        public async Task<UserView> VerifyPin(PinDto pinDto)
        {
            User user = new User();
            UserView users = new UserView();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return null;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var userData = await _context.Users
                .Where(pinInfo => pinInfo.userId == userID).FirstAsync();


                if (!VerifyPinHash(pinDto.Pin, userData.PinHash, userData.PinSalt))
                {
                    users.status = false;
                    users.message = "Incorrect Pin";
                    return users;
                }

                users.status = true;
                users.message = "Correct Pin";
                return users;
            }
            catch (Exception ex)
            {
                users.message = "Check Backend - Verify Pin on Transfer Unsuccessful";
                users.status = false;

                _logger.LogError($"{users.message} \n {ex.Message}");

                return users;
            }
        }

        public async Task<ActionResult<LoginView>> VerifyEmail(string uemail, string utoken)
        {
            LoginView view = new LoginView();

            var checkData = await _context.Users.Where
                (checkDetails => checkDetails.VerificationToken == utoken && checkDetails.Email == uemail).FirstAsync();

            if (checkData == null)
            {
                view.status = false;
                view.message = "Check Backend - Invalid";

                return view;
            }

            //IsVerified
            checkData.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            view.status = true;
            view.message = "Successfully Verified";
            return view;
        }

        public async Task<ActionResult<LoginView>> ForgotPassword(EmailDto email)
        {
            LoginView view = new LoginView();
            ForgotPasswordDto forgotPasswordDto = new ForgotPasswordDto();

            var emailCheck = await _context.Users.Where(e => e.Email == email.Email).FirstAsync();

            if(emailCheck == null)
            {
                view.status = false;
                view.message = "Email not found";

                return view;
            }

            emailCheck.PasswordResetToken = VerficationGen();
            emailCheck.ResetTokenExpires = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();

            forgotPasswordDto.Email = email.Email;
            forgotPasswordDto.Token = emailCheck.PasswordResetToken;
            forgotPasswordDto.Username =emailCheck.Username;

            await _emailService.ForgotPasswordEmail(forgotPasswordDto);

            view.status = true;
            view.message = "email sent";

            return view;
        }

        public async Task<ActionResult<LoginView>> ResetPassword(ResetPassword resetPassword)
        {
            LoginView view = new LoginView();

            if (!resetPassword.Password.Equals(resetPassword.cPassword))
            {
                view.message = "Passwords do not match";
                return view;
            }

            var checkData = await _context.Users.Where
            (checkDetails => checkDetails.PasswordResetToken == resetPassword.Token && checkDetails.Email == resetPassword.Email).FirstAsync();

            if (checkData == null || checkData.ResetTokenExpires < DateTime.Now)
            {
                view.status = false;
                view.message = "Invalid Token";

                return view;
            }

            CreatePasswordHash(resetPassword.Password, out byte[] passwordHash, out byte[] passwordSalt);


            checkData.PasswordHash = passwordHash;
            checkData.PasswordSalt = passwordSalt;
            checkData.PasswordResetToken = null;
            checkData.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            view.status = true;
            view.message = "password changed";

            return view;
        }

        public async Task<object> GetProfile()
        {
            UserView users = new UserView();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new UserProfile();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);


                var userData = await _context.Users
                .Where(userInfo => userInfo.userId == userID)
                .Select(userInfo => new UserProfile
                {
                    Username = userInfo.Username,
                    Email = userInfo.Email,
                    PhoneNumber = userInfo.PhoneNumber,
                    Firstname = userInfo.firstName,
                    Lastname = userInfo.lastName,
                    Address = userInfo.Address
                })
                .FirstOrDefaultAsync();


                if (userData == null) return new UserProfile();


                return userData;
            }
            catch (Exception ex)
            {
                users.message = "Check Backend - Dashboard Display";
                users.status = false;

                _logger.LogError($"{users.message} \n {ex.Message}");

                return users;
            }
        }

        public async Task<object> UpdateProfile(UpdateUserProfile user)
        {
            UserView users = new UserView();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new DashboardView();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var userData = await _context.Users
                .Where(userInfo => userInfo.userId == userID).FirstAsync();


                //var data = await _context.Users.AnyAsync(userCheck => userCheck.Username == user.Username);
                //if (data)
                //{
                //    users.message = "Username exist";
                //    return users;
                //}

                if(user.Firstname == "")
                {
                    user.Firstname = userData.firstName;
                }

                if (user.Lastname == "")
                {
                    user.Lastname = userData.lastName;
                }

                if (user.PhoneNumber == "")
                {
                    user.PhoneNumber = userData.PhoneNumber;
                }

                if (user.Address == "")
                {
                    user.Address = userData.Address;
                }

                userData.firstName = user.Firstname;
                userData.lastName = user.Lastname;
                userData.Username = userData.Username;
                userData.Email = userData.Email;
                userData.PhoneNumber = user.PhoneNumber;
                userData.Address = user.Address;

                await _context.SaveChangesAsync();

                users.status = true;
                users.message = "Updated Succesfully";
                return users;

            }
            catch (Exception ex) 
            {
                return new UserProfile();
            }
        }

        public async Task<object> ProfilePhoto(IFormFile profilePhoto)
        {
            UserView users = new UserView();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new DashboardView();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var userData = await _context.Users
                .Where(userInfo => userInfo.userId == userID).FirstAsync();

                if(userData == null)
                {
                    users.status = false;
                    users.message = "Did not update profile";
                    return users;
                }

                using (var stream = new MemoryStream())
                {
                    profilePhoto.CopyTo(stream);
                    userData.ProfilePhoto = stream.ToArray();
                }

                await _context.SaveChangesAsync();

                users.status = true;
                users.message = "Updated Profile Succesfully";
                return users;

            }
            catch (Exception ex)
            {
                return new UserProfile();
            }
        }

        public async Task<ActionResult<UserView>> CheckSecurityExist()
        {
            UserView users = new UserView();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    users.status = false;
                    return users;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var userData = await _context.securityQuestions
                .Where(securityCheck => securityCheck.userId == userID).FirstAsync();


                if (userData.SecurityQ == null || userData.SecurityA == null)
                {
                    users.status = false;
                    users.message = "Please select a security question and answer";
                    return users;
                }

                users.message = "QA Exists";
                users.status = true;

                return users;
            }
            catch (Exception ex)
            {
                users.message = "Check Backend - Check Security";
                users.status = false;

                _logger.LogError($"{users.message} \n {ex.Message}");

                return users;
            }
        }

        public async Task<UserView> CreateSecurityQA(SecurityDto securityDto)
        {
            UserView users = new UserView();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return null;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var userData = await _context.Users
                .Where(qa => qa.userId == userID).FirstAsync();

                userData.SecurityQuestions.SecurityQ = securityDto.SecurityQ;
                userData.SecurityQuestions.SecurityA = securityDto.SecurityA;

                await _context.SaveChangesAsync();

                users.status = true;
                users.message = "SQ & SA Saved Successfully";
                return users;
            }
            catch (Exception ex)
            {
                users.message = "Check Backend - Create SQ & SA Unsuccessful";
                users.status = false;

                _logger.LogError($"{users.message} \n {ex.Message}");

                return users;
            }
        }

        public async Task<UserView> VerifySecurityQA(SecurityAnswerDto securityDto)
        {
            UserView users = new UserView();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return null;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var userData = await _context.securityQuestions
                .Where(pinInfo => pinInfo.userId == userID).FirstAsync();

                if (userData.SecurityA != securityDto.SecurityA)
                {
                    users.status = false;
                    users.message = "Incorrect Answer, Try Again";
                    return users;
                }

                users.status = true;
                users.message = "Correct Answer";
                return users;
            }
            catch (Exception ex)
            {
                users.message = "Check Backend - Verify Security Answer Unsuccessful";
                users.status = false;

                _logger.LogError($"{users.message} \n {ex.Message}");

                return users;
            }
        }

        public async Task<object> UpdatePin(PinDto pinDto)
        {
            UserView users = new UserView();

            CreatePinHash(pinDto.Pin, out byte[] pinHash, out byte[] pinSalt);
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return null;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var userData = await _context.Users
                .Where(pinInfo => pinInfo.userId == userID).FirstAsync();

                if (pinDto.Pin.Length != 4)
                {
                    users.status = false;
                    users.message = "Your pin must be 4 digits";
                    return users;
                }

                userData.PinHash = pinHash;
                userData.PinSalt = pinSalt;

                await _context.SaveChangesAsync();

                users.status = true;
                users.message = "pin updated";
                return users;
            }
            catch (Exception ex)
            {
                users.message = "Check Backend - Create Pin Unsuccessful";
                users.status = false;

                _logger.LogError($"{users.message} \n {ex.Message}");

                return users;
            }
        }

        public async Task<object> GetPhoto()
        {
            ImageDto users = new ImageDto();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    users.status = false;
                    users.message = "image byte";

                    return users;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var userData = await _context.Users
                .Where(userInfo => userInfo.userId == 1).FirstAsync();


                if (userData.ProfilePhoto == null)
                {
                    users.status = false;
                    users.message = "No Image found";

                    return users;
                }


                users.status = true;
                users.message = "image byte";
                users.data = userData.ProfilePhoto;

                return users;
            }
            catch (Exception ex)
            {
                users.message = "Check Backend - Image Display";
                users.status = false;

                _logger.LogError($"{users.message} \n {ex.Message}");

                return users;
            }
        }

        public async Task<object> GetQuestion()
        {
            UserView users = new UserView();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new DashboardView();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);


                var userData = await _context.securityQuestions
                .Where(userInfo => userInfo.userId == userID)
                .Select(userInfo => new SecurityQuestionDto
                {
                    SecurityQ = userInfo.SecurityQ
                })
                .FirstOrDefaultAsync();


                if (userData == null) return new SecurityAnswerDto();

                users.status = true;
                users.message = userData.SecurityQ;

                return users;
            }
            catch (Exception ex)
            {
                users.message = "Check Backend - Security Answer Display";
                users.status = false;

                _logger.LogError($"{users.message} \n {ex.Message}");

                return users;
            }
        }

        public async Task<object> CreateAvailabeCurrency()
        {
            List<CreateNewWallet> wallet = new List<CreateNewWallet>();
            List<UCreateNewWallet> userWallet = new List<UCreateNewWallet>();
            List<string> AvailableCurrency = new List<string>();

            int userID;
            if (_httpContextAccessor.HttpContext == null)
            {
                return null;
            }

            userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

            var currency = await _context.currencies.ToListAsync();
            var userCurrency = await _context.Accounts.Where(x => x.userId == userID).ToListAsync();
            var test = await _context.currencies.ToListAsync();

            //currency.ForEach(w => wallet.Add(new CreateNewWallet
            //{
            //    Currency = w.Currency,
            //}));

            //userCurrency.ForEach(w => userWallet.Add(new UCreateNewWallet
            //{
            //    Currency = w.Currency,
            //}));            

            /*    foreach(var c in currency)
                {
                    if(userCurrency.Any(x => x.Currency != c.Currency))
                    {
                        AvailableCurrency.Add(c.Currency);
                    }

                }

                return AvailableCurrency;*/

         

            foreach (var c in currency)
            {
                if (userCurrency.Any(x => x.Currency == c.Currency))
                {
                 test.Remove(c);
                }
              /* foreach(var cur in AvailableCurrency)
                {
                    test.RemoveAll(x=> x.Currency !="boom");
                 var Curre =  await _context.currencies.Where(x => x.Currency == cur).FirstOrDefaultAsync();
                    test.Add(Curre);
                }*/
                //test = AvailableCurrency;

            }

            return test; 


            //foreach(var c in currency)
            //{
            //    foreach ( var e in userCurrency)
            //    {
            //        if (e.Currency.Contains(c.Currency))
            //        {

            //        }
            //    }
            //}
        }

        public async Task<UserView> CreateNewWallet(string currency)
        {
            UserView view = new UserView();
            int userID;
            if (_httpContextAccessor.HttpContext == null)
            {
                return null;
            }

            userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

            var x = await _context.Accounts.Where(x => x.userId == userID && x.Currency == currency).FirstOrDefaultAsync();
            if(x != null)
            {
                view.status = false;
                view.message = "This account aleardy exists";
                return view;
            }
            
            var data = await _context.Accounts.Where(x => x.userId == userID && x.Currency == "NGN").FirstOrDefaultAsync();
            var c = await _context.currencies.Where(x => x.Currency == currency).FirstOrDefaultAsync();
            if(data.Balance < c.chargeRate)
            {
                view.status = false;
                view.message = "Insufficient funds to create this account";
                return view;
            }
            
            var myTransaction = _context.Database.BeginTransaction();

            try
            {
                data.Balance = data.Balance - c.chargeRate;

                Account account = new Account
                {
                    AccountNo = AccountGen(),
                    Balance = 0,
                    Currency = currency,
                    userId = userID
                };

                await _context.Accounts.AddAsync(account);
                await _context.SaveChangesAsync();
                await myTransaction.CommitAsync();

            }
            catch (Exception ex)
            {
                await  myTransaction.RollbackAsync();
            }


            view.status = true;
            view.message = $"{currency} account created successfully";
            return view;
        }

        public async Task<object> GetConversion()
        {
            var x = await _context.currencies.ToListAsync();

            return x;
        }

        public async Task<object> CurrentUserForChat()
        {
            int userID;
            if (_httpContextAccessor.HttpContext == null)
            {
                return null;
            }

            userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

            var user = await _context.Users.Where(x => x.userId == userID).Select(x => new UserProfile
            {
                Username = x.Username,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Firstname = x.firstName,
                Lastname = x.lastName,
                Address = x.Address
            }).FirstOrDefaultAsync();

            if (user == null) return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };

            return user;
        }
    }
}





