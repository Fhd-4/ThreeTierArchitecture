namespace Project.DAL.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    public Product(int id, string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("اسم المنتج لا يمكن أن يكون فارغاً");

        if (price < 0)
            throw new ArgumentException("السعر لا يمكن أن يكون سالباً");

        Id = id;
        Name = name;
        Price = price;
    }
}