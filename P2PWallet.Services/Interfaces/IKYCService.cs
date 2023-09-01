﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P2PWallet.Models.DataObjects.KYCDto;

namespace P2PWallet.Services.Interfaces
{
    public interface IKYCService
    {
        Task<object> CreateNewKYCField(NewKYCField newKYC);
    }
}
