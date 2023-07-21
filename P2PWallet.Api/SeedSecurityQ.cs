using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;

namespace P2PWallet.Api
{
    public static class SeedSecurityQ
    {
        public static WebApplication SeedSecurityQs(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                try
                {
                    context.Database.EnsureCreated();

                    var seed = context.seedSecurityQuestions.FirstOrDefault();
                    if (seed == null)
                    {
                        context.seedSecurityQuestions.AddRange(
                                new SeedSecurityQuestion
                                {
                                    SecurityQuestion = "In what city were you born?"
                                },
                                new SeedSecurityQuestion
                                {
                                    SecurityQuestion = "What was the make of your first car?"
                                },
                                new SeedSecurityQuestion
                                {
                                    SecurityQuestion = "What was your favorite food as a child?"
                                },
                                new SeedSecurityQuestion
                                {
                                    SecurityQuestion = "What is your mother's maiden name?"
                                },
                                new SeedSecurityQuestion
                                {
                                    SecurityQuestion = "What year was your father born?"
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
