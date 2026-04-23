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

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Car> Cars { get; set; } = null!;
        public DbSet<UserCars> UserCars { get; set; } = null!;
        public DbSet<Make> Makes { get; set; } = null!;
        public DbSet<Model> Models { get; set; } = null!;

        


        public DbSet<Part> Parts { get; set; } = null!;
        public DbSet<PartCategory> PartCategories { get; set; } = null!;


        //Клиенти и фирми
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<IndividualCustomer> IndividualCustomers { get; set; } = null!;
        public DbSet<LegalEntityCustomer> LegalEntityCustomers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 2. РЕЛАЦИЯ: Car -> Parts (1 към много)
            modelBuilder.Entity<Part>()
                .HasOne(p => p.Car)
                .WithMany(c => c.CarParts)
                .HasForeignKey(p => p.CarId)
                .OnDelete(DeleteBehavior.Cascade); // Трием колата -> трием историята на частите

            // 3. РЕЛАЦИЯ: PartCategory -> Parts (1 към много)
            modelBuilder.Entity<Part>()
                .HasOne(p => p.Category)
                .WithMany(pc => pc.ServiceParts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Не позволяваме триене на категория, ако има части в нея

            // Твоите Seed Data Категории
            modelBuilder.Entity<PartCategory>().HasData(
                new PartCategory { Id = 1, Name = "Двигател и Периферия" },
                new PartCategory { Id = 2, Name = "Спирачна система" },
                new PartCategory { Id = 3, Name = "Ходова част и Окачване" },
                new PartCategory { Id = 4, Name = "Кормилно управление" },
                new PartCategory { Id = 5, Name = "Трансмисия и Съединител" },
                new PartCategory { Id = 6, Name = "Филтри, Масла и Течности" },
                new PartCategory { Id = 7, Name = "Електрическа система и Запалване" },
                new PartCategory { Id = 8, Name = "Горивна система" },
                new PartCategory { Id = 9, Name = "Охладителна система" },
                new PartCategory { Id = 10, Name = "Климатична система" },
                new PartCategory { Id = 11, Name = "Изпускателна система" },
                new PartCategory { Id = 12, Name = "Каросерия и Остъкляване" },
                new PartCategory { Id = 13, Name = "Светлини и Оптика" },
                new PartCategory { Id = 14, Name = "Интериор и Оборудване" },
                new PartCategory { Id = 15, Name = "Гуми и Джанти" },
                new PartCategory { Id = 16, Name = "Аксесоари и Други" }
            );

            // Твоите съществуващи конфигурации за UserCars, Vin и Models
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

            modelBuilder.Entity<Model>()
                .HasOne(m => m.Make)
                .WithMany(mk => mk.Models)
                .HasForeignKey(m => m.MakeId);


            // --- СЕКЦИЯ КЛИЕНТИ (TPT) ---
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<IndividualCustomer>().ToTable("IndividualCustomers");
            modelBuilder.Entity<LegalEntityCustomer>().ToTable("LegalEntityCustomers");

            modelBuilder.Entity<Car>()
                .HasOne(c => c.Customer)
                .WithMany(cust => cust.Cars)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}