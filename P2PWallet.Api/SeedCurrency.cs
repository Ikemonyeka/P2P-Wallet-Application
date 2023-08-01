using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;

namespace P2PWallet.Api
{
    public static class SeedCurrency
    {
        public static WebApplication currencySeed(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                try
                {
                    context.Database.EnsureCreated();

                    var seed = context.currencies.FirstOrDefault();
                    if (seed == null)
                    {
                        context.currencies.AddRange(
                             new Currencies
                             {
                                 Currency = "NGN",
                                 conversionRate = 0,
                                 chargeRate = 0,
                             },
                             new Currencies
                             {
                                 Currency = "USD",
                                 conversionRate = 792,
                                 chargeRate = 2500,
                             },
                             new Currencies
                             {
                                 Currency = "EUR",
                                 conversionRate = 880,
                                 chargeRate = 2500,
                             }, 
                             new Currencies
                             {
                                 Currency = "GBP",
                                 conversionRate = 1019,
                                 chargeRate = 2500,
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
