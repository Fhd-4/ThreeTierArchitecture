using Microsoft.EntityFrameworkCore;
using Project.DAL.Data;
using Project.DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project.DAL.Models.Project>> GetAllProjectsAsync(int? portfolioId, int? programId, string? keyword, int? status)
        {
            var query = _context.Projects
                .Include(p => p.Portfolio)
                .Include(p => p.Program)
                .Include(p => p.Tasks)
                .Include(p => p.ProjectMembers)
                .AsQueryable();

            if (portfolioId.HasValue)
                query = query.Where(p => p.PortfolioId == portfolioId.Value);

            if (programId.HasValue)
                query = query.Where(p => p.ProgramId == programId.Value);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(p => p.Name.Contains(keyword) || (p.Description != null && p.Description.Contains(keyword)));

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            return await query.ToListAsync();
        }

        public async Task<Project.DAL.Models.Project?> GetByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Portfolio)
                .Include(p => p.Program)
                .Include(p => p.Tasks)
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Project.DAL.Models.Project project)
        {
            await _context.Projects.AddAsync(project);
        }

        public void Update(Project.DAL.Models.Project project)
        {
            _context.Projects.Update(project);
        }

        public void Delete(Project.DAL.Models.Project project)
        {
            _context.Projects.Remove(project);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
