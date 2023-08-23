using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.DataObjects;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.AdminDto;
using static P2PWallet.Models.DataObjects.GLDto;

namespace P2PWallet.Services.Services
{
    public class GLService : IGLService
    {
        private readonly DataContext _context;

        public GLService(DataContext context)
        {
            _context = context;
        }

        private static string GLAccountGen()
        {
            var now = DateTime.Now;
            var random = new Random().Next(11111111, 99999999);
            var AcctNumber = random;
            return AcctNumber.ToString();
        }
        public async Task<ResponseMessageModel<bool>> CreateGL(CreateGL createGL)
        {
            try
            {
                var isExists = await _context.generalLedgers.Where(x => x.GLName == createGL.glName).FirstOrDefaultAsync();

                if (isExists != null) return new ResponseMessageModel<bool> { status = false, message = "GL Name already exists", data = false };

                GeneralLedger ledger = new GeneralLedger
                {
                    GLName = createGL.glName,
                    GLAccountNo = $"GL{GLAccountGen()}",
                    Balance = 0,
                    Currency = createGL.glCurrency
                };

                await _context.generalLedgers.AddAsync(ledger);
                await _context.SaveChangesAsync();

                return new ResponseMessageModel<bool> { status = true, message = "GL successfully created", data = true };
            }
            catch
            {
                return new ResponseMessageModel<bool> { status = false, message = "Check System", data = false };
            }
        }
    }
}
