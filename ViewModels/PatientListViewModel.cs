using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.ViewModels;

public class PatientListViewModel
{
    public List<PatientDto> Patients { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public string? Sort { get; set; }
    public string? Dir { get; set; }
    public string? Search { get; set; }
}
