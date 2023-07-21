using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.UserObject;

namespace P2PWallet.Services.Interfaces
{
    public interface ISeedSecurityQuestion
    {
        Task<List<SeededQ>> GetQuestions();
    }
}
