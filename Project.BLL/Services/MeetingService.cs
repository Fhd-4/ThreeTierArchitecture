using Project.BLL.DTOs;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public class MeetingService : IMeetingService
{
    private readonly IMeetingRepository _repo;

    public MeetingService(IMeetingRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<MeetingDetailsDto>> GetMeetingsAsync(int? projectId, string? keyword)
    {
        var meetings = await _repo.GetAllAsync(projectId, keyword);
        return meetings.Select(m => MapToDetailsDto(m));
    }

    public async Task<MeetingDetailsDto?> GetMeetingByIdAsync(int id)
    {
        var meeting = await _repo.GetByIdAsync(id);
        return meeting != null ? MapToDetailsDto(meeting) : null;
    }

    public async Task<MeetingDetailsDto?> CreateMeetingAsync(CreateMeetingDto dto)
    {
        var projectExists = await _repo.ProjectExistsAsync(dto.ProjectId);
        if (!projectExists) return null;

        var meeting = new ProjectMeeting
        {
            Title = dto.Title,
            Date = dto.Date,
            Time = dto.Time,
            MeetingLink = dto.MeetingLink,
            Description = dto.Description,
            Status = dto.Status ?? "Pending",
            InvitedMembers = dto.InvitedMembers,
            AttachedFiles = dto.AttachedFiles,
            ProjectId = dto.ProjectId,
            CreatedDate = DateTime.UtcNow
        };

        await _repo.AddAsync(meeting);
        await _repo.SaveChangesAsync();

        var createdMeeting = await _repo.GetByIdAsync(meeting.Id);
        return MapToDetailsDto(createdMeeting ?? meeting);
    }

    public async Task<bool> UpdateMeetingAsync(int id, UpdateMeetingDto dto)
    {
        var meeting = await _repo.GetByIdAsync(id);
        if (meeting == null) return false;

        var projectExists = await _repo.ProjectExistsAsync(dto.ProjectId);
        if (!projectExists) return false;

        meeting.Title = dto.Title;
        meeting.Date = dto.Date;
        meeting.Time = dto.Time;
        meeting.MeetingLink = dto.MeetingLink;
        meeting.Description = dto.Description;
        meeting.Status = dto.Status ?? meeting.Status;
        meeting.InvitedMembers = dto.InvitedMembers;
        meeting.AttachedFiles = dto.AttachedFiles;
        meeting.ProjectId = dto.ProjectId;

        _repo.Update(meeting);
        return await _repo.SaveChangesAsync();
    }

    public async Task<bool> DeleteMeetingAsync(int id)
    {
        var meeting = await _repo.GetByIdAsync(id);
        if (meeting == null) return false;

        _repo.Delete(meeting);
        return await _repo.SaveChangesAsync();
    }

    // Helper: Map Entity to Details DTO
    private MeetingDetailsDto MapToDetailsDto(ProjectMeeting m)
    {
        return new MeetingDetailsDto
        {
            Id = m.Id,
            Title = m.Title,
            Date = m.Date,
            Time = m.Time,
            MeetingLink = m.MeetingLink,
            Description = m.Description,
            Status = m.Status,
            InvitedMembers = m.InvitedMembers,
            AttachedFiles = m.AttachedFiles,
            ProjectId = m.ProjectId,
            ProjectName = m.Project != null ? m.Project.Name : "N/A",
            CreatedDate = m.CreatedDate
        };
    }
}
