using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<PaystackFund> PaystackFunds { get; set;}
        public DbSet<SecurityQuestion> securityQuestions { get; set; }
        public DbSet<SeedSecurityQuestion> seedSecurityQuestions { get; set; }
    }
}
