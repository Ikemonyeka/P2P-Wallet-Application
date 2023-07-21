using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Org.BouncyCastle.Utilities;
using P2PWallet.Models.DataObjects;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static NPOI.HSSF.Util.HSSFColor;

namespace P2PWallet.Services.Services
{
    public class ExcelService : IExcelService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public ExcelService(DataContext context, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }

        private void CreateCell(IRow CurrentRow, int CellIndex, string Value, XSSFCellStyle Style)
        {
            ICell Cell = CurrentRow.CreateCell(CellIndex);
            Cell.SetCellValue(Value);
            Cell.CellStyle = Style;
        }
        public async Task<byte[]> GetExcel(PdfDto pdfDto)
        {
            int userID;
            if (_httpContextAccessor.HttpContext == null)
            {
                return null;
            }

            userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.SerialNumber)?.Value);

            var x = await _context.Users.Where(x => x.userId == userID).FirstAsync();

            var Y = await _context.Accounts.Where(y => y.userId == userID).FirstAsync();


            var list = await _context.Transfers
            .Where(t => t.DebitUserId == userID || t.BeneficiaryUserId == userID).Where(t => t.Date >= pdfDto.StartDate && t.Date <= pdfDto.EndDate)
            .Select(t => new PdfView
            {
                Date = t.Date,
                BeneficiaryAccountNo = t.BeneficiaryAccountNo,
                DUsername = t.DebitUser.firstName + " " + t.DebitUser.lastName,
                CUsername = t.BeneficiaryUser.firstName + " " + t.BeneficiaryUser.lastName,
                Amount = t.Amount,
                DebitUserId = t.DebitUserId,
                BeneficiaryUserId = t.BeneficiaryUserId
            })
            .ToListAsync();

            foreach (var i in list)
            {
                if (i.Date >= pdfDto.StartDate && i.Date <= pdfDto.EndDate)
                {
                    if (i.DebitUserId == null && i.BeneficiaryUserId == userID)
                    {
                        i.TransactionType = "Credit";
                        i.DUsername = "Paystack Funding";
                    }
                    if (i.DebitUserId == userID)
                    {
                        i.TransactionType = "Debit";
                    }
                    else
                    {
                        i.TransactionType = "Credit";
                    }
                }
            }

            IWorkbook workbook = new XSSFWorkbook();

            ISheet sheet = workbook.CreateSheet("AccountSheet1");

            XSSFFont HeaderFonts = (XSSFFont)workbook.CreateFont();
            HeaderFonts.FontHeightInPoints = 14;
            HeaderFonts.FontName = "Arial";
            HeaderFonts.IsBold = true; //make cell bold

            //create cellStyle
            XSSFCellStyle cellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            cellStyle.SetFont(HeaderFonts);

            //IRow Data = sheet.CreateRow(0);
            //CreateCell(Data, 0, "P2PBanking", cellStyle);
            //IRow Data2 = sheet.CreateRow(1);
            //CreateCell(Data, 1, "Tel No: +(234) 8037429102", cellStyle);
            //IRow Data3 = sheet.CreateRow(2);
            //CreateCell(Data, 2, "Email: customerservice@p2pbanking.com", cellStyle);

            //IRow userData1 = sheet.CreateRow(4);
            //CreateCell(Data, 3, $"From: {pdfDto.StartDate.ToString("MM/dd/yyyy")} - {pdfDto.EndDate.ToString("MM/dd/yyyy")}", cellStyle);
            //IRow userData2 = sheet.CreateRow(5);
            //CreateCell(Data, 4, $"Account No: {Y.AccountNo}", cellStyle);
            //IRow userData3 = sheet.CreateRow(6);
            //CreateCell(Data, 5, $"Currency: {Y.Currency}", cellStyle);
            //IRow userData4 = sheet.CreateRow(7);
            //CreateCell(Data, 6, $"Opening Balance: Coming Soon", cellStyle);

            //IRow userData5 = sheet.CreateRow(9);
            //CreateCell(Data, 7, $"{x.Username}", cellStyle);
            //IRow userData6 = sheet.CreateRow(11);
            //CreateCell(Data, 8, $"{x.Address}", cellStyle);

            //Create The Headers of the excel
            IRow HeaderRow = sheet.CreateRow(12);

            //Create The Actual Cells
            CreateCell(HeaderRow, 0, "Date", cellStyle);
            CreateCell(HeaderRow, 1, "Transaction Type", cellStyle);
            CreateCell(HeaderRow, 2, "Sender", cellStyle);
            CreateCell(HeaderRow, 3, "Receiver", cellStyle);
            CreateCell(HeaderRow, 4, "Amount", cellStyle);

            //basic Font style for all other rows
            XSSFFont BasicFonts = (XSSFFont)workbook.CreateFont();
            BasicFonts.FontHeightInPoints = 14;
            BasicFonts.FontName = "Arial";
            BasicFonts.IsBold = false;

            XSSFCellStyle basiccellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            basiccellStyle.SetFont(BasicFonts);

            var j = 13;
            for (var i = 0; i < list.Count; i++)
            {
                IRow row = sheet.CreateRow(j);

                //add values in cell
                CreateCell(row, 0, $"{list[i].Date.ToString("MM/dd/yyyy")}", basiccellStyle);
                CreateCell(row, 1, $"{list[i].TransactionType}", basiccellStyle);
                CreateCell(row, 2, $"{list[i].DUsername}", basiccellStyle);
                CreateCell(row, 3, $"{list[i].CUsername}", basiccellStyle);
                CreateCell(row, 4, $"{list[i].Amount}", basiccellStyle);
                j++;
            }

            var file = Path.Combine(Directory.GetCurrentDirectory(), "Exports", $"AccStat_{x.Username}.xlsx");

            for (int i = 0; i <= 20; i++) sheet.AutoSizeColumn(i);

            using (FileStream stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
              workbook.Write(stream);
            }
            byte[] data = File.ReadAllBytes(file);


            PdfDate pdfDate = new PdfDate
            {
                EndDate = pdfDto.EndDate,
                StartDate = pdfDto.StartDate,
            };

            if (pdfDto.File == "emailExcel")
            {
                await _emailService.AccountStatement(x.Email, x.Username, pdfDate, file);
            }

            return data;

        }
    }
}



//IWorkbook workbook = new XSSFWorkbook();

//XSSFFont myFont = (XSSFFont)workbook.CreateFont();
//myFont.FontHeightInPoints = 12;
//myFont.FontName = "Tahoma";


//XSSFCellStyle borderedCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
//borderedCellStyle.SetFont(myFont);
//borderedCellStyle.BorderLeft = BorderStyle.Medium;
//borderedCellStyle.BorderTop = BorderStyle.Medium;
//borderedCellStyle.BorderRight = BorderStyle.Medium;
//borderedCellStyle.BorderBottom = BorderStyle.Medium;
//borderedCellStyle.VerticalAlignment = VerticalAlignment.Center;

//ISheet sheet = workbook.CreateSheet("AccountSheet1");

//for (int i = 0; i < list.Count; i++)
//{
//    IRow row = sheet.CreateRow(0);
//    ICell cell = row.CreateCell(0);
//    cell.SetCellValue(list[i].Date);
//    cell.SetCellValue(list[i].TransactionType);
//    cell.SetCellValue(list[i].DUsername);
//    cell.SetCellValue(list[i].CUsername);
//    cell.SetCellValue(((double)list[i].Amount));
//}

//using (FileStream stream = new FileStream($"AccStat_{x.Username}.xlsx", FileMode.Create, FileAccess.Write))
//{
//    workbook.Write(stream);
//}

