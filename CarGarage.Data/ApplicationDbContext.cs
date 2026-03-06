using CarGarage.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; } = null!;
        public DbSet<UserCars> UserCars { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Many-to-many конфигурация за User ↔ Car чрез UserCars
            modelBuilder.Entity<UserCars>()
                .HasKey(uc => new { uc.UserId, uc.CarId });

            modelBuilder.Entity<UserCars>()
                .HasOne(uc => uc.User)
                .WithMany()  // IdentityUser няма да има navigation към UserCars (не е нужно)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCars>()
                .HasOne(uc => uc.Car)
                .WithMany(c => c.UserCars)
                .HasForeignKey(uc => uc.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            // Уникален VIN в Cars (за да няма дублирани автомобили в базата)
            modelBuilder.Entity<Car>()
                .HasIndex(c => c.Vin)
                .IsUnique();
        }
    }
}