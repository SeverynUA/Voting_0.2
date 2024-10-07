using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Models.Voting_m;
using Voting_0._2.Models.Voting_m.SetUp;

namespace Voting_0._2.Data.Entities
{
    public class UserDbContext : IdentityDbContext<Account>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        // У вас є один DbSet для всіх користувачів
        public DbSet<Account> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Інші налаштування, якщо необхідно
        }
    }

}
