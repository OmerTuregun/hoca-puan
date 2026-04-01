using HocaPuan.Core.DTOs.Common;
using HocaPuan.Core.DTOs.Professor;

namespace HocaPuan.Core.Interfaces.Services;

public interface IProfessorService
{
    Task<ProfessorDetailDto?> GetByIdAsync(int id);
    Task<PagedResultDto<ProfessorListItemDto>> SearchAsync(ProfessorSearchDto searchDto);
    Task<ProfessorDetailDto> CreateAsync(CreateProfessorDto dto);
    Task<ProfessorDetailDto> UpdateAsync(int id, UpdateProfessorDto dto);
    Task<bool> DeleteAsync(int id);
    Task RecalculateStatsAsync(int professorId);
}
