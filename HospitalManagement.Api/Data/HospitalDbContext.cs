using HospitalManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Api.Data
{
    public class HospitalDbContext : DbContext
    {
        public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options) { }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Nurse> Nurses { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<User> Users { get; set; }

        // Add other DbSets...

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Admin User
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FullName = "System Admin",
                Email = "admin@hospital.com",
                PasswordHash = "$2a$11$/mJoeeFSjm.g8KTfROsJl.VUM2pibpmtuMCtZo1Cs6QvDvSwCWsI2",
                Role = "Admin"
            });

            // Unique constraint on Email across all users
            modelBuilder.Entity<User>() // New Update code
                .HasIndex(u => u.Email)
                .IsUnique();

            // Doctor ↔ User
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId);

            // Nurse ↔ User
            modelBuilder.Entity<Nurse>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId);

            // Patient ↔ User
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId);

            // Doctor ↔ Assigned Patients
            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Patients)
                .WithOne(p => p.AssignedDoctor)
                .HasForeignKey(p => p.AssignedDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor ↔ Assigned Nurses
            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Nurses)
                .WithOne(n => n.AssignedDoctor)
                .HasForeignKey(n => n.AssignedDoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
