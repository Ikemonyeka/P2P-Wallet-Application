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
        public DbSet<Currencies> currencies { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<LockedUnlockedUserDescription> Descriptions { get; set; }
        public DbSet<LockedUnlockedAccountsDescriptions> LockedUnlockedDescriptions { get; set; }
        public DbSet<GeneralLedger> generalLedgers { get; set; }
        public DbSet<Chat> Chat { get; set; }
        public DbSet<KYCRequiredDocuments> KYCRequiredDocuments { get; set; }
        public DbSet<KYCUpload> kYCUploads { get; set; }
    }
}
