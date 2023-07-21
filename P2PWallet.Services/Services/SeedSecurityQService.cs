using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.DataObjects;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.UserObject;

namespace P2PWallet.Services.Services
{
    public class SeedSecurityQService : ISeedSecurityQuestion
    {
        private readonly DataContext _context;

        public SeedSecurityQService(DataContext context)
        {
            _context = context;
        }
        public async Task<List<SeededQ>> GetQuestions()
        {
            List<SeededQ> questions = new List<SeededQ>();

            try
            {
                var data = await _context.seedSecurityQuestions.ToListAsync();

                data.ForEach(question => questions.Add(new SeededQ
                {
                    Id = question.Id,
                    SecurityQuestion = question.SecurityQuestion

                }));
                return questions;

            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return questions;
            }
        }
    }
}
