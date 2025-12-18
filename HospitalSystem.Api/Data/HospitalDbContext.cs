using HospitalSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalSystem.Api.Data
{
    public class HospitalDbContext : DbContext
    {
        public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Seed Admin User
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FullName = "System Admin",
                Email = "admin@hospital.com",
                PasswordHash = "$2a$11$g3xPPfcMydAXB4JXzkYNW./ks8bj8XkrxtslR4KXoXL3qVWRgqg1u",
                Role = "Admin",
                IsSelfRegistered = false,
                IsSystemAccount = true
            });

            // Unique constraint on Email across all users
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}