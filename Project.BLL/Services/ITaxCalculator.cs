namespace Project.BLL.Services;

public interface ITaxCalculator
{
    decimal CalculateTotalWithTax(decimal basePrice);
}