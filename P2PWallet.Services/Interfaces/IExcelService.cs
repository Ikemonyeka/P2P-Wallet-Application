﻿using P2PWallet.Models.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interfaces
{
    public interface IExcelService
    {
        Task<byte[]> GetExcel(PdfDto pdfDto);
    }
}
