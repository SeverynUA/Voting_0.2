using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Models.Voting_m.Candidat_m;
using Voting_0._2.Models.Voting_m.SetUp;
using Voting_0._2.Models.Voting_m;
using Microsoft.AspNetCore.Identity;

namespace Voting_0._2.Data.Entities
{
    public static class SeedData
    {
            public static async Task InitializeAsync(IServiceProvider serviceProvider, IWebHostEnvironment environment)
            {
                using (var context = serviceProvider.GetRequiredService<VotingDbContext>())
                {
                    var userManager = serviceProvider.GetRequiredService<UserManager<Account>>();

                    // Переконайтеся, що база даних створена
                    context.Database.EnsureCreated();

                    // Перевіряємо, чи є вже записи в таблиці голосувань
                    if (!context.Votings.Any())
                    {
                        // Перевіряємо, чи існує організатор з певним email
                        var organizator = await userManager.FindByEmailAsync("test@example.com");

                        // Якщо організатор не існує, створюємо його
                        if (organizator == null)
                        {
                            organizator = new Account
                            {
                                FullName = "Організатор 1",
                                Email = "test@example.com",
                                UserName = "test@example.com"
                            };

                            var createResult = await userManager.CreateAsync(organizator, "Organizer@123");
                            if (createResult.Succeeded)
                            {
                                await userManager.AddToRoleAsync(organizator, "Organizator"); // Додаємо роль організатора
                            }
                        }

                        // Додаємо голосування для організатора
                        var voting1 = new Voting
                        {
                            Name = "Вибори президента",
                            AccessKey = "PREZ2024",
                            VotingDuration = TimeSpan.FromHours(3),
                            NumberOfVoters = 100,
                            Organizator = organizator,
                            OrganizatorId = organizator.Id, // Прив'язка до зареєстрованого організатора
                            VotingSystem = new VotingSystem(VotingMode.Standard),
                            Candidates = new List<Candidat>
                    {
                        new Candidat { Name = "Кандидат 1", Description = "Опис кандидата 1", VoteCount = 0 },
                        new Candidat { Name = "Кандидат 2", Description = "Опис кандидата 2", VoteCount = 0 }
                    }
                        };

                        var voting2 = new Voting
                        {
                            Name = "Мер міста",
                            AccessKey = "MAYOR2024",
                            VotingDuration = TimeSpan.FromHours(2),
                            NumberOfVoters = null, // Відкрите голосування
                            Organizator = organizator,
                            OrganizatorId = organizator.Id,
                            VotingSystem = new VotingSystem(VotingMode.Elimination),
                            Candidates = new List<Candidat>
                    {
                        new Candidat { Name = "Кандидат A", Description = "Опис кандидата A", VoteCount = 0 },
                        new Candidat { Name = "Кандидат B", Description = "Опис кандидата B", VoteCount = 0 }
                    }
                        };

                        // Додаємо голосування до контексту
                        context.Votings.AddRange(voting1, voting2);

                        await context.SaveChangesAsync();
                    }
                }
            }

    }
}
