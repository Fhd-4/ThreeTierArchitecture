using Microsoft.EntityFrameworkCore;
using Project.DAL.Data;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool hasPortfolios, bool hasPrograms, bool hasProjects, bool hasTasks, bool hasChangeRequests)> CheckUserDependenciesAsync(string userId)
    {
        var hasPortfolios = await _context.Portfolios.AnyAsync(p => p.OwnerId == userId);
        var hasPrograms = await _context.Programs.AnyAsync(p => p.ManagerId == userId);
        var hasProjects = await _context.Projects.AnyAsync(p => p.ManagerId == userId);
        var hasTasks = await _context.Tasks.AnyAsync(t => t.AssigneeId == userId);
        var hasChangeRequests = await _context.ChangeRequests.AnyAsync(cr => cr.RequestedById == userId);

        return (hasPortfolios, hasPrograms, hasProjects, hasTasks, hasChangeRequests);
    }

    public async Task CleanUserDependenciesAsync(string userId)
    {
        var comments = await _context.ChangeRequestComments.Where(c => c.UserId == userId).ToListAsync();
        _context.ChangeRequestComments.RemoveRange(comments);

        var reactions = await _context.MessageReactions.Where(r => r.UserId == userId || (r.Message != null && r.Message.SenderId == userId)).ToListAsync();
        _context.MessageReactions.RemoveRange(reactions);

        var readStates = await _context.MessageReadStates.Where(r => r.UserId == userId || (r.Message != null && r.Message.SenderId == userId)).ToListAsync();
        _context.MessageReadStates.RemoveRange(readStates);

        var messages = await _context.ChatMessages.Where(m => m.SenderId == userId).ToListAsync();
        _context.ChatMessages.RemoveRange(messages);

        var memberships = await _context.ProjectMembers.Where(m => m.UserId == userId).ToListAsync();
        _context.ProjectMembers.RemoveRange(memberships);

        var approvedCRs = await _context.ChangeRequests.Where(cr => cr.ApprovedById == userId).ToListAsync();
        foreach (var cr in approvedCRs)
        {
            cr.ApprovedById = null;
        }

        await _context.SaveChangesAsync();
    }

    public async Task AssociateUserWithEntitiesAsync(string userId, int? portfolioId, int? programId, int? projectId)
    {
        if (portfolioId.HasValue)
        {
            var portfolio = await _context.Portfolios.FindAsync(portfolioId.Value);
            if (portfolio != null)
            {
                portfolio.OwnerId = userId;
                _context.Entry(portfolio).State = EntityState.Modified;
            }
        }

        if (programId.HasValue)
        {
            var program = await _context.Programs.FindAsync(programId.Value);
            if (program != null)
            {
                program.ManagerId = userId;
                _context.Entry(program).State = EntityState.Modified;
            }
        }

        if (projectId.HasValue)
        {
            var project = await _context.Projects.FindAsync(projectId.Value);
            if (project != null)
            {
                project.ManagerId = userId;
                _context.Entry(project).State = EntityState.Modified;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<Portfolio>> GetPortfoliosByOwnerIdAsync(string ownerId)
    {
        return await _context.Portfolios
            .Include(p => p.Programs)
            .Include(p => p.Projects)
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<List<ProjectProgram>> GetProgramsByManagerIdAsync(string managerId)
    {
        return await _context.Programs
            .Include(p => p.Projects)
            .Where(p => p.ManagerId == managerId)
            .ToListAsync();
    }

    public async Task<List<Project.DAL.Entities.Project>> GetProjectsByManagerIdAsync(string managerId)
    {
        return await _context.Projects
            .Include(p => p.Tasks)
            .Where(p => p.ManagerId == managerId)
            .ToListAsync();
    }
}
