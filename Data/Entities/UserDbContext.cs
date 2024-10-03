using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Models.Voting_m;
using Voting_0._2.Models.Voting_m.SetUp;

namespace Voting_0._2.Data.Entities
{
    public class UserDbContext : IdentityDbContext<IdentityUser>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        public DbSet<Account> AdminUsers { get; set; }
        public DbSet<Account> Organizators { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Voting>()
            //.OwnsOne(v => v.VotingSystem);

        }
    }
}
