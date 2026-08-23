using Microsoft.EntityFrameworkCore;
using Project.DAL.Entities;

namespace Project.DAL.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // وضع بيانات أولية داخل قاعدة البيانات (Data Seeding)
        modelBuilder.Entity<Product>().HasData(
            new Product(1, "Laptop", 3500),
            new Product(2, "Mouse", 150)
        );
    }
}
