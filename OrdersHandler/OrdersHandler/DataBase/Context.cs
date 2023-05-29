using Microsoft.EntityFrameworkCore;
using OrdersHandler.Models;

namespace OrdersHandler.Data
{
    public class Context: DbContext
    {
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDish> OrderDishes { get; set; }

        public Context(DbContextOptions<Context> options) : base(options) 
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("Identity");
            builder.Entity<Dish>(entity =>
            {
                entity.ToTable(name: "Dish");
            });
            builder.Entity<Order>(entity =>
            {
                entity.ToTable(name: "Order");
            });
            builder.Entity<OrderDish>(entity =>
            {
                entity.ToTable(name: "OrderDish");
            });
        }
    }
}
