namespace Project.BLL.Services;

public class SaudiVatCalculator : ITaxCalculator
{
    private const decimal VatRate = 0.15m;

    public decimal CalculateTotalWithTax(decimal basePrice) => basePrice * (1 + VatRate);
}