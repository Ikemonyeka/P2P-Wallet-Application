using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.AdminDto;
using static P2PWallet.Models.DataObjects.GLDto;

namespace P2PWallet.Services.Interfaces
{
    public interface IGLService
    {
        Task<ResponseMessageModel<bool>> CreateGL(CreateGL createGL);
    }
}
