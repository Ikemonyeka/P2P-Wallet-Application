﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using P2PWallet.Services.Data;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("P2PWallet.Models.Entities.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AccountNo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("userId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("userId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.Admin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsLoggedIn")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("firstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("lastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("phoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Admin");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.Chat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("adminId")
                        .HasColumnType("int");

                    b.Property<DateTime>("date")
                        .HasColumnType("datetime2");

                    b.Property<bool>("isReadAdmin")
                        .HasColumnType("bit");

                    b.Property<bool>("isReadUser")
                        .HasColumnType("bit");

                    b.Property<string>("message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("messageType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("userId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("adminId");

                    b.HasIndex("userId");

                    b.ToTable("Chat");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.Currencies", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("chargeRate")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("conversionRate")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.ToTable("currencies");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.GeneralLedger", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GLAccountNo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GLName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("generalLedgers");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.LockedUnlockedAccountsDescriptions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("DescriptionId")
                        .HasColumnType("int");

                    b.Property<int>("userId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DescriptionId");

                    b.HasIndex("userId");

                    b.ToTable("LockedUnlockedDescriptions");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.LockedUnlockedUserDescription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Descriptions");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.Notifications", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("userId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("userId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.PaystackFund", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Reference")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("userId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("userId");

                    b.ToTable("PaystackFunds");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.SecurityQuestion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("SecurityA")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SecurityQ")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("userId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("userId")
                        .IsUnique();

                    b.ToTable("securityQuestions");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.SeedSecurityQuestion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("SecurityQuestion")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("seedSecurityQuestions");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.Transfer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("BeneficiaryAccountNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("BeneficiaryUserId")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("DebitAccountNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DebitUserId")
                        .HasColumnType("int");

                    b.Property<string>("Reference")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BeneficiaryUserId");

                    b.HasIndex("DebitUserId");

                    b.ToTable("Transfers");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.User", b =>
                {
                    b.Property<int>("userId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("userId"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("PasswordResetToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("PinHash")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("PinSalt")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("ProfilePhoto")
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTime?>("ResetTokenExpires")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("VerificationToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("VerifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("firstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("lastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("userId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.Account", b =>
                {
                    b.HasOne("P2PWallet.Models.Entities.User", "User")
                        .WithMany("Account")
                        .HasForeignKey("userId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.Chat", b =>
                {
                    b.HasOne("P2PWallet.Models.Entities.Admin", "Admin")
                        .WithMany("Chats")
                        .HasForeignKey("adminId");

                    b.HasOne("P2PWallet.Models.Entities.User", "User")
                        .WithMany("Chats")
                        .HasForeignKey("userId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Admin");

                    b.Navigation("User");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.LockedUnlockedAccountsDescriptions", b =>
                {
                    b.HasOne("P2PWallet.Models.Entities.LockedUnlockedUserDescription", "LockedUnlockedUser")
                        .WithMany("LockedUnlockedDescriptions")
                        .HasForeignKey("DescriptionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("P2PWallet.Models.Entities.User", "User")
                        .WithMany("LockedUnlockedDescriptions")
                        .HasForeignKey("userId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LockedUnlockedUser");

                    b.Navigation("User");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.Notifications", b =>
                {
                    b.HasOne("P2PWallet.Models.Entities.User", "User")
                        .WithMany("Notifications")
                        .HasForeignKey("userId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.PaystackFund", b =>
                {
                    b.HasOne("P2PWallet.Models.Entities.User", "User")
                        .WithMany("PaystackFunds")
                        .HasForeignKey("userId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.SecurityQuestion", b =>
                {
                    b.HasOne("P2PWallet.Models.Entities.User", "User")
                        .WithOne("SecurityQuestions")
                        .HasForeignKey("P2PWallet.Models.Entities.SecurityQuestion", "userId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.Transfer", b =>
                {
                    b.HasOne("P2PWallet.Models.Entities.User", "BeneficiaryUser")
                        .WithMany()
                        .HasForeignKey("BeneficiaryUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("P2PWallet.Models.Entities.User", "DebitUser")
                        .WithMany("Transfers")
                        .HasForeignKey("DebitUserId");

                    b.Navigation("BeneficiaryUser");

                    b.Navigation("DebitUser");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.Admin", b =>
                {
                    b.Navigation("Chats");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.LockedUnlockedUserDescription", b =>
                {
                    b.Navigation("LockedUnlockedDescriptions");
                });

            modelBuilder.Entity("P2PWallet.Models.Entities.User", b =>
                {
                    b.Navigation("Account");

                    b.Navigation("Chats");

                    b.Navigation("LockedUnlockedDescriptions");

                    b.Navigation("Notifications");

                    b.Navigation("PaystackFunds");

                    b.Navigation("SecurityQuestions")
                        .IsRequired();

                    b.Navigation("Transfers");
                });
#pragma warning restore 612, 618
        }
    }
}
