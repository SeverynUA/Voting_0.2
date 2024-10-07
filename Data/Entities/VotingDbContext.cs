using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Models.ViewModels;
using Voting_0._2.Models.Voting_m;
using Voting_0._2.Models.Voting_m.Candidat_m;
using Voting_0._2.Models.Voting_m.SetUp;

namespace Voting_0._2.Data.Entities
{
    public class VotingDbContext : DbContext
    {
        public VotingDbContext(DbContextOptions<VotingDbContext> options) : base(options)
        {
        }

        public DbSet<Voting> Votings { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Candidat> Candidates { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Voter_User> Voters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Voting>().OwnsOne(v => v.VotingSystem);

            modelBuilder.Entity<Candidat>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                // Зв'язок з таблицею Image (один до одного)
                entity.HasOne(e => e.Image)
                    .WithOne(e => e.Candidat)
                    .HasForeignKey<Image>(e => e.CandidatID)
                    .OnDelete(DeleteBehavior.Cascade); // Видалення кандидата призведе до видалення зображення
            });

            // Конфігурація для Image
            modelBuilder.Entity<Image>(entity =>
            {
                entity.Property(e => e.FilePath)
                    .HasMaxLength(200);

                entity.HasOne(e => e.Candidat)
                    .WithOne(e => e.Image)
                    .HasForeignKey<Image>(e => e.CandidatID)
                    .OnDelete(DeleteBehavior.Cascade); // Видалення зображення при видаленні кандидата
            });

            modelBuilder.Entity<Voting>()
                .HasOne(v => v.Organizator) // Встановлюємо, що голосування має одного організатора
                .WithMany()
                .HasForeignKey(v => v.OrganizatorId) // Встановлюємо зовнішній ключ для зв'язку
                .OnDelete(DeleteBehavior.Restrict); // Видалення організатора не видаляє голосування

        }
    }
}
