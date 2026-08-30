using Project.BLL.DTOs;
using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public interface IProgramService
{
    Task<IEnumerable<ProgramDetailsDto>> GetProgramsAsync(int? portfolioId, string? keyword, int? status);
    Task<ProgramDetailsDto?> GetProgramByIdAsync(int id);
    Task<ProjectProgram> CreateProgramAsync(CreateProgramDto dto);
    Task<ProjectProgram?> UpdateProgramAsync(int id, UpdateProgramDto dto);
    Task<(bool Success, string? ErrorMessage)> DeleteProgramAsync(int id);
}