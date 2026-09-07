using Microsoft.EntityFrameworkCore;
using Project.DAL.Data;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync(string? keyword)
    {
        var query = _context.Categories.AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(c => c.Name.Contains(keyword));
        }

        return await query.ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
    }

    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }

    public void Delete(Category category)
    {
        _context.Categories.Remove(category);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
