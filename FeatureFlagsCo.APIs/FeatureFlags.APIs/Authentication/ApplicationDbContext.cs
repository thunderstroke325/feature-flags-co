using FeatureFlags.APIs.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.APIs.Authentication
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountUserMapping> AccountUserMappings { get; set; }
        public DbSet<UserInvitation> UserInvitations { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUserMapping> ProjectUserMappings { get; set; }
        public DbSet<Environment> Environments { get; set; }
    }
}