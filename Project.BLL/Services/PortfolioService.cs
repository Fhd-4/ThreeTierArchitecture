using Project.BLL.DTOs;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _repo;

    public PortfolioService(IPortfolioRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<PortfolioDetailsDto>> GetPortfoliosAsync()
    {
        var portfolios = await _repo.GetAllAsync();
        return portfolios.Select(p => MapToDetailsDto(p));
    }

    public async Task<PortfolioDetailsDto?> GetPortfolioByIdAsync(int id)
    {
        var portfolio = await _repo.GetByIdAsync(id);
        return portfolio != null ? MapToDetailsDto(portfolio) : null;
    }

    public async Task<PortfolioDetailsDto> CreatePortfolioAsync(CreatePortfolioDto dto, string? userId)
    {
        var portfolio = new Portfolio
        {
            Name = dto.Name ?? dto.NameAr ?? string.Empty,
            Description = dto.Description ?? dto.DescriptionAr,
            Budget = dto.Budget,
            Category = dto.Category ?? string.Empty,
            Status = MapStatusStringToInt(dto.Status),
            SponsorName = dto.SponsorName ?? string.Empty,
            ManagerName = dto.ManagerName ?? string.Empty,
            OwnerName = dto.OwnerName ?? string.Empty,
            OwnerId = userId ?? "default-user-id",
            AttachedFiles = dto.AttachedFiles,
            CreatedDate = DateTime.UtcNow
        };

        await _repo.AddAsync(portfolio);
        await _repo.SaveChangesAsync();

        // Fetch created portfolio again to ensure Navigation Properties are populated
        var createdPortfolio = await _repo.GetByIdAsync(portfolio.Id);
        return MapToDetailsDto(createdPortfolio ?? portfolio);
    }

    public async Task<bool> UpdatePortfolioAsync(int id, UpdatePortfolioDto dto)
    {
        var portfolio = await _repo.GetByIdAsync(id);
        if (portfolio == null) return false;

        portfolio.Name = dto.Name ?? dto.NameAr ?? portfolio.Name;
        portfolio.Description = dto.Description ?? dto.DescriptionAr ?? portfolio.Description;
        portfolio.Budget = dto.Budget;
        portfolio.Category = dto.Category ?? portfolio.Category;
        portfolio.Status = MapStatusStringToInt(dto.Status);
        portfolio.SponsorName = dto.SponsorName ?? portfolio.SponsorName;
        portfolio.ManagerName = dto.ManagerName ?? portfolio.ManagerName;
        portfolio.OwnerName = dto.OwnerName ?? portfolio.OwnerName;
        portfolio.AttachedFiles = dto.AttachedFiles ?? portfolio.AttachedFiles;

        _repo.Update(portfolio);
        return await _repo.SaveChangesAsync();
    }

    public async Task<bool> DeletePortfolioAsync(int id)
    {
        var portfolio = await _repo.GetByIdAsync(id);
        if (portfolio == null) return false;

        _repo.Delete(portfolio);
        return await _repo.SaveChangesAsync();
    }

    public async Task<IEnumerable<object>> GetUsersForTestAsync()
    {
        var users = await _repo.GetUsersForTestAsync();
        return users.Select(u => new
        {
            u.Id,
            u.UserName,
            u.Email,
            u.NameAr,
            u.NameEn
        });
    }

    public async Task<object> GetStatsAsync()
    {
        return await _repo.GetStatsAsync();
    }

    // Helper Status Mapper
    private int MapStatusStringToInt(string? statusStr)
    {
        if (string.IsNullOrEmpty(statusStr)) return 1;
        switch (statusStr.ToLower())
        {
            case "active":
            case "نشط":
                return 1;
            case "completed":
            case "مكتمل":
                return 2;
            case "pending":
            case "onhold":
            case "قيد الانتظار":
                return 3;
            case "rejected":
            case "refusing":
            case "مرفوض":
                return 4;
            default:
                return 1;
        }
    }

    // Helper Map to Details DTO
    private PortfolioDetailsDto MapToDetailsDto(Portfolio p)
    {
        return new PortfolioDetailsDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Budget = p.Budget,
            Category = p.Category,
            Status = p.Status == 1 ? "Active" :
                     p.Status == 2 ? "Completed" :
                     p.Status == 3 ? "OnHold" : "Rejected",
            SponsorName = p.SponsorName,
            ManagerName = p.ManagerName,
            CreatedDate = p.CreatedDate,
            OwnerName = !string.IsNullOrEmpty(p.OwnerName) ? p.OwnerName : (p.Owner != null ? p.Owner.UserName : null),
            OwnerId = p.OwnerId,
            AttachedFiles = p.AttachedFiles,
            ProgramsCount = p.Programs?.Count ?? 0,
            ProjectsCount = p.Projects?.Count ?? 0
        };
    }
}
