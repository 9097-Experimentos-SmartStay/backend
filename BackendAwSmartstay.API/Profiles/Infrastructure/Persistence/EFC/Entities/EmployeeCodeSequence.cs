namespace BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Entities;

/// <summary>
/// Infrastructure entity for atomic sequence generation of Staff Employee Codes (EMP-XXXXX).
/// </summary>
public class EmployeeCodeSequence
{
    public int Id { get; set; } = 1;
    public int LastValue { get; set; } = 0;
}
