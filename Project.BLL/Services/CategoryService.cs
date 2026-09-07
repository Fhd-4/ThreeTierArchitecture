using Project.BLL.DTOs;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync(string? keyword)
    {
        return await _repo.GetAllAsync(keyword);
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _repo.GetByIdAsync(id);
    }

    public async Task<Category?> CreateCategoryAsync(CategoryDto dto)
    {
        if (string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.AssignTo))
        {
            return null;
        }

        var category = new Category
        {
            Name = dto.Name,
            AssignTo = dto.AssignTo
        };

        await _repo.AddAsync(category);
        await _repo.SaveChangesAsync();

        return category;
    }

    public async Task<Category?> UpdateCategoryAsync(int id, CategoryDto dto)
    {
        var category = await _repo.GetByIdAsync(id);
        if (category == null) return null;

        category.Name = dto.Name;
        category.AssignTo = dto.AssignTo;

        _repo.Update(category);
        await _repo.SaveChangesAsync();

        return category;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _repo.GetByIdAsync(id);
        if (category == null) return false;

        _repo.Delete(category);
        return await _repo.SaveChangesAsync();
    }
}
