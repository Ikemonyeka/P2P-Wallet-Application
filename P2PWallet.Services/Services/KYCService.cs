using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using Org.BouncyCastle.Math.EC.Rfc7748;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.KYCDto;
using static P2PWallet.Models.DataObjects.UserObject;

namespace P2PWallet.Services.Services
{
    public class KYCService : IKYCService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConverter _convert;
        private readonly IHubContext<SignalHub> _hub;

        public KYCService(DataContext context, IHttpContextAccessor httpContextAccessor, IConverter convert, IHubContext<SignalHub> hub)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _convert = convert;
            _hub = hub;
        }
        public async Task<object> CreateNewKYCField(NewKYCField newKYC)
        {
            try
            {
                var isExist = await _context.KYCRequiredDocuments.Where(x => x.NameOfDocument.ToLower() == newKYC.documentName.ToLower()).FirstOrDefaultAsync();
                if(isExist != null) return new ResponseMessageModel<bool> { status = false, message = "already exists", data = false };
                
                var code = await _context.KYCRequiredDocuments.OrderByDescending(x => x.id).FirstAsync();
                char formCode = Convert.ToChar(code.FormCode);
                char letter = (char)(formCode + 1);

                //code.FormCode = "A";

                KYCRequiredDocuments requiredDocuments = new KYCRequiredDocuments
                {
                    NameOfDocument = newKYC.documentName,
                    FormCode = letter.ToString(),
                    isEnabled = true
                };

                await _context.KYCRequiredDocuments.AddAsync(requiredDocuments);
                await _context.SaveChangesAsync();

                return new ResponseMessageModel<bool> { status = true, message = "New Required Field Created", data = true };
            }
            catch
            {
                return new ResponseMessageModel<bool> { status = false, message = "System back check", data = false };
            }
        }

        public async Task<object> GetKYCRequirements()
        {
            int userID;
            if (_httpContextAccessor.HttpContext == null) return new ResponseMessageModel<bool> { status = false, message = "no user data", data = false };

            userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

            var user = await _context.kYCUploads.Where(x => x.Status != false && x.userId == userID).ToListAsync();

            var list = await _context.KYCRequiredDocuments.Where(x => x.isEnabled == true).ToListAsync();

            var missingDocuments = list
            .Where(requiredDoc => !user.Any(userDoc => userDoc.KycRecId == requiredDoc.id)).ToList();

            return missingDocuments;
        }

        public async Task<object> NewKYCUpload(newUpload upload)
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null) return new ResponseMessageModel<bool> { status = false, message = "no user data", data = false };

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

                var user = await _context.Users.Where(x => x.userId == userID).FirstOrDefaultAsync();

                if (user == null) return new ResponseMessageModel<bool> { status = false, message = "no user data", data = false };

                var doc = await _context.KYCRequiredDocuments.Where(x => x.FormCode == upload.formCode).FirstOrDefaultAsync();

                if (doc == null) return new ResponseMessageModel<bool> { status = false, message = "doc not found", data = false };

                var file = upload.file;

                var path = Path.Combine(Directory.GetCurrentDirectory(), "kycUploads");

                var fileName = $"{upload.formCode}{user.Username}";

                using(Stream stream = File.Create($"{path}//{fileName}"))
                {
                    upload.file.CopyTo(stream);
                    stream.Flush();
                }

                var document = await _context.kYCUploads.Where(x => x.userId == userID && x.KycRecId == doc.id).FirstOrDefaultAsync();

                if(document != null)
                {
                    document.date = DateTime.Now;
                    document.PathName = fileName;
                    document.FileType = file.ContentType;
                    document.userId = userID;
                    document.KycRecId = doc.id;
                    document.Status = null;

                    await _context.SaveChangesAsync();

                    return new ResponseMessageModel<bool> { status = true, message = "Updated file to new file", data = true };
                }

                KYCUpload kYCUpload = new KYCUpload
                {
                    date = DateTime.Now,
                    PathName = fileName,
                    FileType = file.ContentType,
                    userId = userID,
                    KycRecId = doc.id
                };

                await _context.AddAsync(kYCUpload);
                await _context.SaveChangesAsync();

                return new ResponseMessageModel<bool> { status = true, message = "Uploaded new file", data = true };
            }
            catch
            {
                return new ResponseMessageModel<bool> { status = false, message = "system check", data = false };
            }
        }

        public async Task<object> GetListKYC()
        {
            var kycList = await _context.KYCRequiredDocuments.Where(x => x.isEnabled == true).ToListAsync();

            //var change = await _context.KYCRequiredDocuments.Where(x => x.id == 2).FirstOrDefaultAsync();
            //change.isEnabled = true;
            //await _context.SaveChangesAsync();

            return kycList;
        }

        public async Task<object> RemoveKYCReq(string code)
        {
            var req = await _context.KYCRequiredDocuments.Where(x => x.FormCode == code).FirstOrDefaultAsync();
            if(req == null) return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };
            
            req.isEnabled = false;

            await _context.SaveChangesAsync();

            return new ResponseMessageModel<bool> { status = true, message = "Item has successfully been disabled", data = true };

        }

        public async Task<object> KYCPending(int page)
        {
            var pending = await _context.kYCUploads.Where(x => x.Status == null).Select(x => new AdminKYCView
            {
                username = x.User.Username,
                formCode = x.KYCRequiredDocuments.FormCode,
                file = Path.Combine(Directory.GetCurrentDirectory(), "kycUploads", x.PathName),
                pendingFile = null
            }).GroupBy(x => x.username).ToDictionaryAsync(group => group.Key, group => group.ToList());

           foreach(var kyc in pending)
            {
                

                foreach (var pend in kyc.Value)
                {
                    byte[] imageBytes = File.ReadAllBytes(pend.file);
                    string base64String = Convert.ToBase64String(imageBytes);

                    pend.pendingCount = kyc.Value.Count();

                    pend.pendingFile = imageBytes;
                }
            }

            var pageResults = 2f;
            var pageSize = Math.Ceiling(pending.Count() / pageResults);

            var paginatedData = new
            {
                Page = page,
                PageSize = pageSize,
                //TotalCount = pending.Count,
                Data = pending
         .Skip((page - 1) * (int)pageResults)
         .Take((int)pageResults)
         .ToDictionary(entry => entry.Key, entry => entry.Value).ToArray()
            };


            var data = pending.ToArray();

            //pending.GroupBy(x => x.username).ToDictionary(group => group.Key, group => group.ToList());
            //.GroupBy(x => x.username).ToDictionaryAsync(group => group.Key, group => group.ToList())

            //var pageResults = 2f;

            //var dataPage = Math.Ceiling(pending.Count() / pageResults);

            //var data = pending.Skip((page - 1) * (int)pageResults).Take((int)pageResults).ToArray();

            //if (data == null) return new ResponseMessageModel<bool> { status = false, message = $"No user found", data = false };

            //var response = new Pagination
            //{
            //    users = data.ToDictionary(
            //            key => key.Key,
            //            value => value.Value),
            //    pages = (int)dataPage,
            //    currentpage = page
            //};


            return paginatedData;
        }

        public async Task<object> AcceptUpload(AcceptUserUpload upload)
        {
            try
            {
                var user = await _context.Users.Where(x => x.Username == upload.username).FirstOrDefaultAsync();
                if (user == null) return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };

                var req = await _context.KYCRequiredDocuments.Where(x => x.FormCode == upload.formCode).FirstOrDefaultAsync();
                if (req == null) return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };

                var uploadedDoc = await _context.kYCUploads.Where(x => x.KycRecId == req.id && x.userId == user.userId).FirstOrDefaultAsync();

                if (uploadedDoc == null) return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };

                uploadedDoc.Status = true;
                uploadedDoc.Reason = "Accepted";

                await _context.SaveChangesAsync();

                var completedUpload = await _context.kYCUploads.Where(x => x.userId == user.userId && x.Status == true).ToListAsync();
                var allReq = await _context.KYCRequiredDocuments.Where(x => x.isEnabled == true).ToListAsync();

                if (completedUpload.Count() >= allReq.Count())
                {
                    user.isKYCVerified = true;

                    await _context.SaveChangesAsync();

                    var forexMessage = $"Locate the '+' sign on the NGN card to create a FOREX currency";

                    Notifications forexNotification = new Notifications
                    {
                        Date = DateTime.Now,
                        Title = $"{upload.formCode} - {req.NameOfDocument} has been approved",
                        Message =forexMessage,
                        Status = false,
                        userId = user.userId
                    };

                    await _context.Notifications.AddAsync(forexNotification);

                    await _context.SaveChangesAsync();

                    await _hub.Clients.All.SendAsync("ForexEnabled", upload.username, forexMessage);
                }

                Notifications notification = new Notifications
                {
                    Date = DateTime.Now,
                    Title = $"{upload.formCode} - {req.NameOfDocument} has been approved",
                    Message = $"Hi {user.Username}, your {upload.formCode} - {req.NameOfDocument} upload has been accepted",
                    Status = false,
                    userId = user.userId
                };

                await _context.Notifications.AddAsync(notification);

                await _context.SaveChangesAsync();

                await _hub.Clients.All.SendAsync("AcceptedSignal", upload.username, notification.Message);

                return new ResponseMessageModel<bool> { status = true, message = "user upload has been approved", data = true };

            }
            catch
            {
                return new ResponseMessageModel<bool> { status = false, message = "system back check uploads", data = false };
            }
        }

        public async Task<object> RejectUpload(RejectUserUpload upload)
        {
            try
            {
                var user = await _context.Users.Where(x => x.Username == upload.username).FirstOrDefaultAsync();
                if (user == null) return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };

                var req = await _context.KYCRequiredDocuments.Where(x => x.FormCode == upload.formCode).FirstOrDefaultAsync();
                if (req == null) return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };

                var uploadedDoc = await _context.kYCUploads.Where(x => x.KycRecId == req.id && x.userId == user.userId).FirstOrDefaultAsync();

                if (uploadedDoc == null) return new ResponseMessageModel<bool> { status = false, message = "no data", data = false };

                uploadedDoc.Status = false;
                uploadedDoc.Reason = upload.reason;

                await _context.SaveChangesAsync();

                Notifications notification = new Notifications
                {
                    Date = DateTime.Now,
                    Title = $"{upload.formCode} - {req.NameOfDocument} has been rejected",
                    Message = $"Hi {user.Username}, your {upload.formCode} - {req.NameOfDocument} upload has been rejected, Reason: {upload.reason}, kindly re-upload",
                    Status = false,
                    userId = user.userId
                };

                await _context.Notifications.AddAsync(notification);

                await _context.SaveChangesAsync();

                var message = $"{upload.formCode} - {req.NameOfDocument} has been rejected";

                await _hub.Clients.All.SendAsync("RejectedSignal", upload.username, message);

                return new ResponseMessageModel<bool> { status = true, message = "user upload has been rejected", data = true };

            }
            catch
            {
                return new ResponseMessageModel<bool> { status = false, message = "system back check uploads", data = false };
            }
        }
    }
}
