namespace Project.DAL.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    // مشيد خاص لـ Entity Framework لتمكينه من قراءة الكائن من قاعدة البيانات دون التحقق من صحة المدخلات مجدداً
    protected Product()
    {
        Name = null!;
    }

    public Product(string name, decimal price)
    {
        ValidateDetails(name, price);
        Name = name;
        Price = price;
    }

    public Product(int id, string name, decimal price) : this(name, price)
    {
        Id = id;
    }

    // طريقة لتحديث التفاصيل مع الحفاظ على مبادئ OOP (الكبسلة Encapsulation والتحقق من صحة البيانات)
    public void UpdateDetails(string name, decimal price)
    {
        ValidateDetails(name, price);
        Name = name;
        Price = price;
    }

    private void ValidateDetails(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("اسم المنتج لا يمكن أن يكون فارغاً");

        if (price < 0)
            throw new ArgumentException("السعر لا يمكن أن يكون سالباً");
    }
}