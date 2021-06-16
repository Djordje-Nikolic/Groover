using Groover.DB.MySqlDb.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.DB.MySqlDb
{
    public class GrooverDbContext : IdentityDbContext<User, Role, int>
    {
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<GroupUser> GroupUsers { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

        public GrooverDbContext(DbContextOptions<GrooverDbContext> options)
               : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<GroupUser>()
                .HasKey(bc => new { bc.GroupId, bc.UserId });

            builder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .IsRequired();

            builder.Entity<Group>()
                .HasIndex(g => g.Name)
                .IsUnique();

        }
    }
}
