using Project.BLL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public interface IMeetingService
{
    Task<IEnumerable<MeetingDetailsDto>> GetMeetingsAsync(int? projectId, string? keyword);
    Task<MeetingDetailsDto?> GetMeetingByIdAsync(int id);
    Task<MeetingDetailsDto?> CreateMeetingAsync(CreateMeetingDto dto);
    Task<bool> UpdateMeetingAsync(int id, UpdateMeetingDto dto);
    Task<bool> DeleteMeetingAsync(int id);
}
