namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

public readonly record struct GuestProfileId(Guid Value)
{
    public static GuestProfileId New() => new(Guid.NewGuid());
}

public readonly record struct StaffProfileId(Guid Value)
{
    public static StaffProfileId New() => new(Guid.NewGuid());
}

public readonly record struct AssignmentId(Guid Value)
{
    public static AssignmentId New() => new(Guid.NewGuid());
}

public readonly record struct UserId(int Value);
public readonly record struct TargetId(int Value);
