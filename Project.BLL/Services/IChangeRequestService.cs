using Project.BLL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public interface IChangeRequestService
{
    Task<IEnumerable<ChangeRequestDetailsDto>> GetChangeRequestsAsync(int? projectId, string? keyword);
    Task<ChangeRequestDetailsDto?> GetChangeRequestByIdAsync(int id);
    Task<ChangeRequestDetailsDto?> CreateChangeRequestAsync(CreateChangeRequestDto dto, string? userId);
    Task<bool> UpdateChangeRequestAsync(int id, UpdateChangeRequestDto dto);
    Task<bool> ApproveChangeRequestAsync(int id, string? approvedById);
    Task<bool> RejectChangeRequestAsync(int id, string? approvedById);
    Task<bool> DeleteChangeRequestAsync(int id);
    
    // Comments
    Task<IEnumerable<CommentDto>> GetCommentsAsync(int requestId);
    Task<CommentDto> CreateCommentAsync(CreateCommentDto dto, string? userId);
}
