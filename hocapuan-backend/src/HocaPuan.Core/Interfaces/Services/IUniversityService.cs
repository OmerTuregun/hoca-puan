using HocaPuan.Core.DTOs.University;

namespace HocaPuan.Core.Interfaces.Services;

public interface IUniversityService
{
    Task<UniversityDetailDto?> GetByIdAsync(int id);
    Task<List<UniversityListItemDto>> GetAllAsync(string? search = null);
    Task<UniversityDetailDto> CreateAsync(CreateUniversityDto dto);
    Task<UniversityDetailDto> UpdateAsync(int id, CreateUniversityDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<FacultyDto>> GetFacultiesAsync(int universityId);
    Task<List<DepartmentDto>> GetDepartmentsAsync(int facultyId);
}
