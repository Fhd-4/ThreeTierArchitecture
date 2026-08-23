namespace Project.BLL.Services;

public interface ITaxCalculator
{
    decimal CalculateTotalWithTax(decimal basePrice);
}

public class SaudiVatCalculator : ITaxCalculator
{
    private const decimal VatRate = 0.15m;

    public decimal CalculateTotalWithTax(decimal basePrice)
    {
        return basePrice * (1 + VatRate);
    }
}