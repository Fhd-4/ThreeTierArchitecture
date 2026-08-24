namespace Project.DAL.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    // مشيد خاص لـ Entity Framework أو محركات البيانات التي تتطلب مشيداً افتراضياً
    protected Product()
    {
        Name = null!;
    }

    // مشيد لإنشاء منتج جديد وتوليد معرف GUID فريد (مستقل عن قاعدة البيانات)
    public Product(string name, decimal price)
    {
        ValidateDetails(name, price);
        Id = Guid.NewGuid().ToString(); // توليد المعرف الفريد هنا
        Name = name;
        Price = price;
    }

    // مشيد يُستخدم عند استرجاع البيانات أو إدخال بيانات أولية محددة المعرف
    public Product(string id, string name, decimal price) : this(name, price)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("معرف المنتج لا يمكن أن يكون فارغاً");

        Id = id;
    }

    // طريقة لتحديث التفاصيل مع الحفاظ على مبادئ OOP (الكبسلة والتحقق من صحة البيانات)
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