using Project.BLL.DTOs;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public class ChangeRequestService : IChangeRequestService
{
    private readonly IChangeRequestRepository _repo;

    public ChangeRequestService(IChangeRequestRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<ChangeRequestDetailsDto>> GetChangeRequestsAsync(int? projectId, string? keyword)
    {
        var crs = await _repo.GetAllAsync(projectId, keyword);
        return crs.Select(cr => MapToDetailsDto(cr));
    }

    public async Task<ChangeRequestDetailsDto?> GetChangeRequestByIdAsync(int id)
    {
        var cr = await _repo.GetByIdAsync(id);
        return cr != null ? MapToDetailsDto(cr) : null;
    }

    public async Task<ChangeRequestDetailsDto?> CreateChangeRequestAsync(CreateChangeRequestDto dto, string? userId)
    {
        var projectExists = await _repo.ProjectExistsAsync(dto.ProjectId);
        if (!projectExists) return null;

        var resolvedUserId = userId;
        if (string.IsNullOrEmpty(resolvedUserId))
        {
            var firstUser = await _repo.GetFirstUserAsync();
            resolvedUserId = firstUser?.Id ?? "default-user-id";
        }

        var cr = new ChangeRequest
        {
            Title = dto.Title,
            Description = dto.Description,
            Reason = dto.Reason,
            ImpactCost = dto.ImpactCost,
            ImpactTimeDays = dto.ImpactTimeDays,
            Status = 1, // 1 = Pending
            ProjectId = dto.ProjectId,
            RequestedById = resolvedUserId,
            RequestDate = DateTime.UtcNow,
            AttachedFiles = dto.AttachedFiles
        };

        await _repo.AddAsync(cr);
        await _repo.SaveChangesAsync();

        var createdCr = await _repo.GetByIdAsync(cr.Id);
        return MapToDetailsDto(createdCr ?? cr);
    }

    public async Task<bool> UpdateChangeRequestAsync(int id, UpdateChangeRequestDto dto)
    {
        var cr = await _repo.GetByIdAsync(id);
        if (cr == null) return false;

        var projectExists = await _repo.ProjectExistsAsync(dto.ProjectId);
        if (!projectExists) return false;

        cr.Title = dto.Title;
        cr.Description = dto.Description;
        cr.Reason = dto.Reason;
        cr.ImpactCost = dto.ImpactCost;
        cr.ImpactTimeDays = dto.ImpactTimeDays;
        cr.ProjectId = dto.ProjectId;
        cr.AttachedFiles = dto.AttachedFiles;

        _repo.Update(cr);
        return await _repo.SaveChangesAsync();
    }

    public async Task<bool> ApproveChangeRequestAsync(int id, string? approvedById)
    {
        var cr = await _repo.GetByIdAsync(id);
        if (cr == null) return false;

        var resolvedUserId = approvedById;
        if (string.IsNullOrEmpty(resolvedUserId))
        {
            var firstUser = await _repo.GetFirstUserAsync();
            resolvedUserId = firstUser?.Id ?? "default-user-id";
        }

        cr.Status = 2; // 2 = Approved
        cr.ApprovedById = resolvedUserId;
        cr.ActionDate = DateTime.UtcNow;

        _repo.Update(cr);
        return await _repo.SaveChangesAsync();
    }

    public async Task<bool> RejectChangeRequestAsync(int id, string? approvedById)
    {
        var cr = await _repo.GetByIdAsync(id);
        if (cr == null) return false;

        var resolvedUserId = approvedById;
        if (string.IsNullOrEmpty(resolvedUserId))
        {
            var firstUser = await _repo.GetFirstUserAsync();
            resolvedUserId = firstUser?.Id ?? "default-user-id";
        }

        cr.Status = 3; // 3 = Rejected
        cr.ApprovedById = resolvedUserId;
        cr.ActionDate = DateTime.UtcNow;

        _repo.Update(cr);
        return await _repo.SaveChangesAsync();
    }

    public async Task<bool> DeleteChangeRequestAsync(int id)
    {
        var cr = await _repo.GetByIdAsync(id);
        if (cr == null) return false;

        _repo.Delete(cr);
        return await _repo.SaveChangesAsync();
    }

    // Comments
    public async Task<IEnumerable<CommentDto>> GetCommentsAsync(int requestId)
    {
        var comments = await _repo.GetCommentsForRequestAsync(requestId);
        return comments.Select(c => new CommentDto
        {
            Id = c.Id,
            ChangeRequestId = c.ChangeRequestId,
            UserId = c.UserId,
            UserName = c.UserName,
            Text = c.Text,
            CreatedDate = c.CreatedDate
        });
    }

    public async Task<CommentDto> CreateCommentAsync(CreateCommentDto dto, string? userId)
    {
        var resolvedUserId = userId;
        string userName = "Abdallah Othman";

        if (string.IsNullOrEmpty(resolvedUserId))
        {
            var firstUser = await _repo.GetFirstUserAsync();
            resolvedUserId = firstUser?.Id ?? "default-user-id";
            userName = firstUser?.UserName ?? "Abdallah Othman";
        }
        else
        {
            var user = await _repo.GetUserByIdAsync(resolvedUserId);
            if (user != null)
            {
                userName = user.UserName ?? "Abdallah Othman";
            }
        }

        var comment = new ChangeRequestComment
        {
            ChangeRequestId = dto.ChangeRequestId,
            UserId = resolvedUserId,
            UserName = userName,
            Text = dto.Text,
            CreatedDate = DateTime.UtcNow
        };

        await _repo.AddCommentAsync(comment);
        await _repo.SaveChangesAsync();

        return new CommentDto
        {
            Id = comment.Id,
            ChangeRequestId = comment.ChangeRequestId,
            UserId = comment.UserId,
            UserName = comment.UserName,
            Text = comment.Text,
            CreatedDate = comment.CreatedDate
        };
    }

    // Helper: Map Entity to Details DTO
    private ChangeRequestDetailsDto MapToDetailsDto(ChangeRequest cr)
    {
        return new ChangeRequestDetailsDto
        {
            Id = cr.Id,
            Title = cr.Title,
            Description = cr.Description,
            Reason = cr.Reason,
            ImpactCost = cr.ImpactCost,
            ImpactTimeDays = cr.ImpactTimeDays,
            Status = cr.Status,
            ProjectId = cr.ProjectId,
            ProjectName = cr.Project != null ? cr.Project.Name : "N/A",
            RequestedById = cr.RequestedById,
            RequestedByUserName = cr.RequestedBy != null ? cr.RequestedBy.UserName ?? "User" : "User",
            ApprovedById = cr.ApprovedById,
            ApprovedByUserName = cr.ApprovedBy != null ? cr.ApprovedBy.UserName : null,
            RequestDate = cr.RequestDate,
            ActionDate = cr.ActionDate,
            AttachedFiles = cr.AttachedFiles
        };
    }
}
