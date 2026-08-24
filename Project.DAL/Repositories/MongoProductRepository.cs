using MongoDB.Driver;
using Project.DAL.Entities;

namespace Project.DAL.Repositories;

public class MongoProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products;

    public MongoProductRepository(IMongoClient mongoClient)
    {
        // الاتصال بقاعدة بيانات Clean3TierDemoDb ومجموعة Products
        var database = mongoClient.GetDatabase("Clean3TierDemoDb");
        _products = database.GetCollection<Product>("Products");

        // إدخال بيانات أولية (Data Seeding) إذا كانت المجموعة فارغة
        SeedData();
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await _products.Find(p => true).ToListAsync(cancellationToken);
        return result.AsReadOnly();
    }

    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _products.Find(p => p.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (product.CreatedAt == default)
        {
            product.CreatedAt = DateTime.UtcNow;
        }
        await _products.InsertOneAsync(product, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _products.ReplaceOneAsync(p => p.Id == product.Id, product, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _products.DeleteOneAsync(p => p.Id == product.Id, cancellationToken: cancellationToken);
    }

    private void SeedData()
    {
        // التحقق مما إذا كانت المجموعة فارغة لحقن البيانات الأولية
        if (_products.EstimatedDocumentCount() == 0)
        {
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var seedProducts = new List<Product>
            {
                new Product("seed-laptop-id", "Labtop", 4000) { CreatedAt = seedDate },
                new Product("seed-mouse-id", "Mouse", 150) { CreatedAt = seedDate }
            };
            _products.InsertMany(seedProducts);
        }
    }
}
