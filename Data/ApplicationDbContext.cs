using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QRFileTrackingapi.Models.Entities;

namespace QRFileTrackingapi.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserAccount, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<RcodeFile> RcodeFiles { get; set; }
        // public DbSet<RcodeFileHistory> RcodeFilesHistory { get; set; } // Uncommented if needed

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Explicitly map Identity tables
            builder.Entity<UserAccount>().ToTable("AspNetUsers");
            builder.Entity<IdentityRole>().ToTable("AspNetRoles");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");

            // Ensure EpfNo is unique
            builder.Entity<UserAccount>()
                .HasIndex(u => u.EpfNo)
                .IsUnique();
        }
    }
}
