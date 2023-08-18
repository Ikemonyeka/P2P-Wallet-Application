using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;

namespace P2PWallet.Api
{
    public static class SeedProfileLUDescription
    {
        public static WebApplication descriptionSeed(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                try
                {
                    context.Database.EnsureCreated();

                    var seed = context.Descriptions.FirstOrDefault();
                    if (seed == null)
                    {
                        context.Descriptions.AddRange(
                             new LockedUnlockedUserDescription
                             {
                                 Description = "Fraud Prevention"
                             },
                             new LockedUnlockedUserDescription
                             {
                                 Description = "Large or Unusual Transactions"
                             },
                             new LockedUnlockedUserDescription
                             {
                                 Description = "Account Inactivity"
                             },
                             new LockedUnlockedUserDescription
                             {
                                 Description = "Regulatory Compliance"
                             },
                             new LockedUnlockedUserDescription
                             {
                                 Description = "Security Concerns"
                             }
                         );

                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                return app;
            }
        }
    }
}
