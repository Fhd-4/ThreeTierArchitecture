using Microsoft.EntityFrameworkCore;
using Project.DAL.Data;
using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly AppDbContext _context;

        public PortfolioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Portfolio>> GetAllAsync()
        {
            return await _context.Portfolios
                .Include(p => p.Owner)
                .Include(p => p.Programs)
                .Include(p => p.Projects)
                .ToListAsync();
        }

        public async Task<Portfolio?> GetByIdAsync(int id)
        {
            return await _context.Portfolios
                .Include(p => p.Owner)
                .Include(p => p.Programs)
                .Include(p => p.Projects)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Portfolio portfolio)
        {
            await _context.Portfolios.AddAsync(portfolio);
        }

        public void Update(Portfolio portfolio)
        {
            _context.Portfolios.Update(portfolio);
        }

        public void Delete(Portfolio portfolio)
        {
            _context.Portfolios.Remove(portfolio);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
