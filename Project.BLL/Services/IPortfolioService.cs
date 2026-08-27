using Project.BLL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public interface IPortfolioService
{
    Task<IEnumerable<PortfolioDetailsDto>> GetPortfoliosAsync();
    Task<PortfolioDetailsDto?> GetPortfolioByIdAsync(int id);
    Task<PortfolioDetailsDto> CreatePortfolioAsync(CreatePortfolioDto dto, string? userId);
    Task<bool> UpdatePortfolioAsync(int id, UpdatePortfolioDto dto);
    Task<bool> DeletePortfolioAsync(int id);
}
