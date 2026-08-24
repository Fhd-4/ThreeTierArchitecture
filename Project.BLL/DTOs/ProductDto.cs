namespace Project.BLL.DTOs;

public class ProductDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal PriceWithVat { get; set; } // السعر مع الضريبة
}