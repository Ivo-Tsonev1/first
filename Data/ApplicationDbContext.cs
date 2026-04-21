using ByteBite.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Menu> Menus { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DiningTable> DiningTables { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Dish.Price decimal precision
            modelBuilder.Entity<Dish>()
                .Property(d => d.Price)
                .HasPrecision(10, 2);

            // Configure Order.TotalPrice decimal precision
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(10, 2);
        }
    }
}
