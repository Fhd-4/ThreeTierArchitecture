using Project.BLL.DTOs;
using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetCategoriesAsync(string? keyword);
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category?> CreateCategoryAsync(CategoryDto dto);
    Task<Category?> UpdateCategoryAsync(int id, CategoryDto dto);
    Task<bool> DeleteCategoryAsync(int id);
}
