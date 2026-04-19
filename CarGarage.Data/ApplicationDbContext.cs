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

        // Нови DbSet-ове с точни имена на таблиците
        public DbSet<Make> Makes { get; set; } = null!;
        public DbSet<Model> Models { get; set; } = null!;
        public DbSet<MakeModel> MakeModels { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Стара конфигурация (не я пипаме)
            modelBuilder.Entity<UserCars>()
                .HasKey(uc => new { uc.UserId, uc.CarId });

            modelBuilder.Entity<UserCars>()
                .HasOne(uc => uc.User)
                .WithMany()
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCars>()
                .HasOne(uc => uc.Car)
                .WithMany(c => c.UserCars)
                .HasForeignKey(uc => uc.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Car>()
                .HasIndex(c => c.Vin)
                .IsUnique();

          

            modelBuilder.Entity<MakeModel>()
                .HasIndex(mm => new { mm.MakeId, mm.ModelId })
                .IsUnique();
        }
    }
}