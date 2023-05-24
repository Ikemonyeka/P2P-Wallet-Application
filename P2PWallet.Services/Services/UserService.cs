using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
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
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.PaystackFundObject;
using static P2PWallet.Models.DataObjects.UserObject;



namespace P2PWallet.Services.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;

        public UserService(DataContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger)
        {
            _context = context;
            _configuration = configuration;
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

        public async Task<UserView> RegisterUser(UserDto user)
        {
            UserView users = new UserView();
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
                    Address = user.Address
                };

                await _context.Users.AddAsync(userInfo);

                var userOutput = await _context.SaveChangesAsync();


                Account newAccount = new Account
                {
                    AccountNo = AccountGen(),
                    Balance = 10000,
                    Currency = "NGN",
                    userId = userInfo.userId
                };

                await _context.Accounts.AddAsync(newAccount);

                var accountOutput = await _context.SaveChangesAsync();
 
                users.message = "Registration Successful, Please check your email to verify your account";
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
                var data = await _context.Users.Where(userCheck => userCheck.Username == user.Username).FirstAsync();
                if (data == null)
                {
                    view.status = false;
                    view.message = "Email or Username exist";
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


                var userData = await _context.Users
                .Where(userInfo => userInfo.userId == userID)
                .Select(userInfo => new DashboardView
                {
                    Username = userInfo.Username,
                    AccountNo = userInfo.Account.AccountNo,
                    Balance = userInfo.Account.Balance,
                    Currency = userInfo.Account.Currency
                })
                .FirstOrDefaultAsync();


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
    }
}





