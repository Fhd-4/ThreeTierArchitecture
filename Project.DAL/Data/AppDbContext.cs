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

        // تحديد دقة وحجم حقل السعر في قاعدة البيانات لمنع فقدان البيانات بعد الفاصلة
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18, 2)");

        // وضع بيانات أولية داخل قاعدة البيانات (Data Seeding)
        // تاريخ ثابت لمنع حدوث خطأ PendingModelChangesWarning بسبب التواريخ الديناميكية
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Product>().HasData(
            new Product(1, "Laptop", 3500) { CreatedAt = seedDate },
            new Product(2, "Mouse", 150) { CreatedAt = seedDate }
        );
    }

    // تعبئة تاريخ الإنشاء تلقائياً عند إضافة سجل جديد (Audit Trail Pattern)
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            if (entity.CreatedAt == default)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
