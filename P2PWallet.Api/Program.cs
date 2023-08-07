using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Services;
using Swashbuckle.AspNetCore.Filters;
using NLog;
using NLog.Web;
using System.Text;
using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.Extensions.FileProviders;
using ServiceStack.Text;
using P2PWallet.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace P2PWallet.Api
{
    public class Program

    {
        public static void Main(string[] args)
        {
            // Early init of NLog to allow startup and exception logging, before host is built
            var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
            logger.Debug("init main");
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add services to the container.

                builder.Services.AddControllers();

                //add signalr
                builder.Services.AddSignalR();

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
                        In = ParameterLocation.Header,
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
                    });

                    options.OperationFilter<SecurityRequirementsOperationFilter>();
                });

                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                            .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
                            ValidateIssuer = false,
                            ValidateAudience = false
                        };
                    });
                builder.Services.AddDbContext<DataContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
                });
                builder.Services.AddScoped<IUserService, UserService>();
                builder.Services.AddScoped<IEmailService, EmailService>();
                builder.Services.AddScoped<ITransferService, TransferService>();
                builder.Services.AddScoped<IPaystackFundService, PaystackFundService>();
                builder.Services.AddScoped<ISeedSecurityQuestion, SeedSecurityQService>();
                builder.Services.AddScoped<INotificationService, NotificationService>();
                builder.Services.AddScoped<IPdfServices, PdfService>();
                builder.Services.AddScoped<IExcelService, ExcelService>();
                builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

                builder.Services.AddHttpContextAccessor();

                // NLog: Setup NLog for Dependency injection
                builder.Logging.ClearProviders();
                builder.Host.UseNLog();


                builder.Services.AddCors((options) =>
                {
                    options.AddPolicy("NotificationClientApp",
                        new CorsPolicyBuilder()
                        .WithOrigins("http://127.0.0.1:5500")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(origin => true)
                        .AllowCredentials()
                        .Build());
                });

                var app = builder.Build();

                app.UseCors("NotificationClientApp");

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());


               

                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();

                app.UseAuthentication();

                app.UseAuthorization();


                app.MapControllers();

                app.currencySeed();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHub<SignalHub>("/alert");
                });

                app.SeedSecurityQs();

                app.Run();
            }
            catch (Exception exception)
            {
                // NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }
    }
}