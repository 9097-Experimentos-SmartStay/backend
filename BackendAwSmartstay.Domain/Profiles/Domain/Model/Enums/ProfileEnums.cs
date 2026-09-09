namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;

public enum ProfileStatus 
{ 
    Active, 
    Inactive 
}

public enum StaffRole 
{ 
    ChainAdmin, 
    Admin, 
    Reception, 
    Housekeeping, 
    Maintenance, 
    Staff 
}

public enum ScopeLevel 
{ 
    Chain, 
    Hotel 
}

public enum HabitualShift 
{ 
    Morning, 
    Afternoon, 
    Night, 
    Rotating 
}

public enum AssignmentStatus 
{ 
    Scheduled, 
    Active, 
    Suspended, 
    Terminated 
}

public enum DocumentType 
{ 
    Dni, 
    Passport, 
    ForeignerId 
}
