namespace Project.BLL.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceWithVat { get; set; } // السعر مع الضريبة
}