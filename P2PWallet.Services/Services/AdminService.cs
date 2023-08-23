using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using NPOI.SS.Formula.Functions;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Migrations;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.AdminDto;

namespace P2PWallet.Services.Services
{
    public class AdminService : IAdminService
    {
        private readonly DataContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminService(DataContext context, IEmailService emailService, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        private string CreateToken(AdminClaims adminClaims)
        {
            List<Claim> claims = new List<Claim>
            {
                 new Claim(ClaimTypes.SerialNumber, adminClaims.userId.ToString("")),
                new Claim(ClaimTypes.Name, adminClaims.Username),
                new Claim(ClaimTypes.Role, adminClaims.Role)

            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;

        }

        private static string VerficationGen()
        {
            const string src = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%&*";
            int length = 8;
            var sb = new StringBuilder();
            Random RNG = new Random();
            for (var i = 0; i < length; i++)
            {
                var c = src[RNG.Next(0, src.Length)];
                sb.Append(c);
            }
            return sb.ToString();
        }
        public async Task<ResponseObject> CreateNewAdmin(AdminR admin)
        {
            ResponseObject response = new ResponseObject();
            try
            {
                var checkExists = await _context.Admin.AnyAsync(x => x.Username == admin.Username || x.Email == admin.Email);
                if (checkExists)
                {
                    response.message = "Email or Username exist";
                    response.status = false;

                    return response; ;
                }
                var pass = VerficationGen();

                CreatePasswordHash(pass, out byte[] passwordHash, out byte[] passwordSalt);

                Admin newAdmin = new Admin
                {
                    Username = admin.Username,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Email = admin.Email,
                    firstName = admin.firstName,
                    lastName = admin.lastName,
                    phoneNumber = admin.PhoneNumber,
                    Role = "Secondary",
                    Status = true,
                    IsLoggedIn = false,

                };

                await _context.Admin.AddAsync(newAdmin);
                await _context.SaveChangesAsync();

                await _emailService.NewAdminEmail(newAdmin.Username, pass, newAdmin.Email);

                response.status = true;
                response.message = "New admin Succesfully created";

                return response;
            }
            catch { 
                return response;
            }
        }

        public async Task<ResponseMessageModel<bool>> LoginAdmin(AdminLogin login)
        {
            //ResponseMessageModel<bool> response = new ResponseMessageModel<bool>();
            AdminClaims adminClaims = new AdminClaims();
            try
            {
                var admin = await _context.Admin.Where(x => x.Username == login.Username).FirstOrDefaultAsync();
                if (admin == null) return new ResponseMessageModel<bool> { status = false, message = "incorrect Username or Password" };

                if (!VerifyPasswordHash(login.Password, admin.PasswordHash, admin.PasswordSalt)) return new ResponseMessageModel<bool> { status = false, message = "Incorrect Username or Password"};

                if(admin.Role == "Secondary" && admin.LastLogin == null) return new ResponseMessageModel<bool> { status = true, message = "Login valid, reset password", data = false };

                adminClaims.userId = admin.Id;
                adminClaims.Username = admin.Username;
                adminClaims.Role = admin.Role;

                var token = CreateToken(adminClaims);

                var response = new ResponseMessageModel<bool> { status = true, message = "Login Successful", dt = token ,data = true };

                return response;

            }
            catch 
            {
                var response = new ResponseMessageModel<bool> { status = false, message = "Login System Error", data = false };

                return response;
            }
        }

        public async Task<object> GetAdmin()
        {
            try
            {
                if (_httpContextAccessor.HttpContext == null)
                {
                    return null;
                }

                var adminRole = _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.Role)?.Value;
                int adminId = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var admin = await _context.Admin.Where(x => x.Role == adminRole && x.Id == adminId)
                    .Select(x => new LoggedInAdmin
                    {
                        Username = x.Username,
                    }).FirstOrDefaultAsync();

                if(admin == null)
                {
                    return null;
                }

                TimeSpan time = DateTime.Now.TimeOfDay;

                if (time < TimeSpan.FromHours(12))
                {
                    admin.TimeOfDay = "Good Morning";
                }
                else if (time < TimeSpan.FromHours(17))
                {
                    admin.TimeOfDay = "Good Afternoon";
                }
                else
                {
                    admin.TimeOfDay = "Good Evening";
                }

                if(adminRole == "Primary")
                {
                    admin.Role = "Super Admin";
                }
                else
                {
                    admin.Role = "Admin";
                }

                return admin;
            }
            catch
            {
                return null;
            }
        }

        public async Task<object> GetCurrencyRate()
        {
            var x = await _context.currencies.ToListAsync();

            return x;
        }

        public async Task<object> GetSummaryTransfersAdmin()
        {
            //var summary = await _context.Transfers.OrderByDescending(x => x.Date).Take(4).ToListAsync();

            //var arrNotes = summary.ToArray();

            //return arrNotes;

            var summary = await _context.Transfers.OrderByDescending(x => x.Date).Take(6)
                .Select(transferInfo => new TransferView
                {
                    Date = transferInfo.Date.ToString("MM/dd/yyyy"),
                    BeneficiaryAccountNo = transferInfo.BeneficiaryAccountNo,
                    DUsername = transferInfo.DebitUser.firstName + " " + transferInfo.DebitUser.lastName,
                    CUsername = transferInfo.BeneficiaryUser.firstName + " " + transferInfo.BeneficiaryUser.lastName,
                    Amount = transferInfo.Amount,
                    Status = transferInfo.Status,
                    DebitUserId = transferInfo.DebitUserId,
                    BeneficiaryUserId = transferInfo.BeneficiaryUserId
                })
                .ToListAsync();

            foreach (var t in summary)
            {
                var x = await _context.Accounts.Where(acc => acc.AccountNo == t.BeneficiaryAccountNo).FirstOrDefaultAsync();
                t.Currency = x.Currency;
            }

            foreach (var transferInfos in summary)
            {
                if (transferInfos.DebitUserId == null)
                {
                    transferInfos.DUsername = "Paystack Funding";
                }
                else
                {
                    transferInfos.TransactionType = "Transfer";
                }
            }

            if (summary == null)
            {
                return new List<TransferView>();
            }

            return summary;
        }

        public async Task<object> GetUsers()
        {
            allUsers u = new allUsers();
            var users = await _context.Users.Where(x => x.Status == false).Select(x => new allUsers
            {
                user = x.Username,
                email = x.Email,
                status = x.Status
            }).ToListAsync();



            foreach (var user in users)
            {

                var c = await _context.Users.Where(x => x.Username == user.user).FirstOrDefaultAsync();
                if (c.VerifiedAt == null)
                {
                    user.dateCreated = "not verified";
                }
                else
                {
                    user.dateCreated = c.VerifiedAt.Value.ToString("MM/dd/yyyy");
                }

            }

            return users;
        }

        public async Task<ResponseMessageModel<bool>> EnableDisableProfile(profileStatus profileStatus)
        {
            var user = await _context.Users.Where(x => x.Username == profileStatus.user && x.Email == profileStatus.email).FirstOrDefaultAsync();

            if(user == null) return new ResponseMessageModel<bool> { status = false, message = $"user cannot be toggled", data = false };

            user.Status = profileStatus.status;

            await _context.SaveChangesAsync();

            return new ResponseMessageModel<bool> { status = true, message = $"This users profile has been set to {profileStatus.status}", data = true };
        }

        public async Task<object> GetDescriptions()
        {
            var d = await _context.Descriptions.ToListAsync();

            return d;
        }

        public async Task<ResponseMessageModel<bool>> DescriptionOfLU(DescriptionLU descriptionLU)
        {
            var user = await _context.Users.Where(x => x.Email == descriptionLU.email && x.Username == descriptionLU.user).FirstOrDefaultAsync();

            if (user == null) return new ResponseMessageModel<bool> { status = false, message = "invalid user request", data = false };

            var storeDesc = await _context.Descriptions.Where(x => x.Description == descriptionLU.description).FirstOrDefaultAsync();

            if (storeDesc == null) return new ResponseMessageModel<bool> { status = false, message = "invalid user request", data = false };

            LockedUnlockedAccountsDescriptions lockedUnlocked = new LockedUnlockedAccountsDescriptions
            {
                DescriptionId = storeDesc.Id,
                userId = user.userId,
                Date = DateTime.UtcNow
            };

            await _context.AddAsync(lockedUnlocked);

            await _context.SaveChangesAsync();

            if (descriptionLU.status == false)
            {
                await _emailService.LockedAccountEmail(descriptionLU.description, descriptionLU.email);
            }
            else{
                await _emailService.UnlockedEmail(descriptionLU.email);
            }

            return new ResponseMessageModel<bool> { status = true, message = "Completed", data = true };
        }

        public async Task<ResponseMessageModel<bool>> SetAdminPassword(AdminPassword adminpassword)
        {
            var admin = await _context.Admin.Where(x => x.Username == adminpassword.username).FirstOrDefaultAsync();

            if(admin == null) return new ResponseMessageModel<bool> { status = false, message = $"username or password incorrect", data = false };
            
            if(adminpassword.password != adminpassword.cpassword) return new ResponseMessageModel<bool> { status = false, message = $"passwords do not match", data = false };

            CreatePasswordHash(adminpassword.password, out byte[] passwordHash, out byte[] passwordSalt);

            admin.PasswordHash = passwordHash;
            admin.PasswordSalt = passwordSalt;
            admin.LastLogin = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ResponseMessageModel<bool> { status = true, message = $"passsword has been updated successfully", data = true };
        }

        public async Task<object> FindUser(string identifier)
        {
            var users = await _context.Users.Where(x => x.Username == identifier || x.firstName == identifier || 
            x.lastName == identifier).FirstOrDefaultAsync();

            if (users == null) return new ResponseMessageModel<bool> { status = false, message = $"No user found", data = false };

            return users;

        }
    }
}
