using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.Domain.Profiles.Domain.Services;

public interface IEmployeeCodeGenerator
{
    Task<EmployeeCode> GenerateNextCodeAsync();
}
