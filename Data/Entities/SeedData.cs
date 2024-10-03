using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Models.Voting_m.Candidat_m;
using Voting_0._2.Models.Voting_m.SetUp;
using Voting_0._2.Models.Voting_m;

namespace Voting_0._2.Data.Entities
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, IWebHostEnvironment environment)
        {
            using (var context = serviceProvider.GetRequiredService<VotingDbContext>())
            {
                // Переконайтеся, що база даних створена
                context.Database.EnsureCreated();

                // Перевіряємо, чи є вже записи в таблиці голосувань
                if (!context.Votings.Any())
                {
                    var organizator = new Account
                    {
                        FullName = "Організатор 1",
                        Email = "organizer1@example.com"
                    };

                    var voting1 = new Voting
                    {
                        Name = "Вибори президента",
                        AccessKey = "PREZ2024",
                        VotingDuration = TimeSpan.FromHours(3),
                        NumberOfVoters = 100,
                        Organizator = organizator,
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
