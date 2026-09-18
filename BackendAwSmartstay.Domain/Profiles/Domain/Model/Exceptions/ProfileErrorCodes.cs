namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.Exceptions;

/// <summary>Stable error codes of the Profiles context (guest and staff profiles).</summary>
public static class ProfileErrorCodes
{
    public const string FirstNameRequired = "profile.first_name_required";
    public const string LastNameRequired = "profile.last_name_required";
    public const string EmailRequired = "profile.email_required";
    public const string EmailInvalid = "profile.email_invalid";
    public const string PhoneRequired = "profile.phone_required";
    public const string PhoneInvalid = "profile.phone_invalid";
    public const string DocumentNumberRequired = "profile.document_number_required";
    public const string DniInvalid = "profile.dni_invalid";
    public const string PassportInvalid = "profile.passport_invalid";
    public const string ForeignerIdInvalid = "profile.foreigner_id_invalid";
    public const string DateRangeInvalid = "profile.date_range_invalid";

    public const string GuestAlreadyLinked = "guest_profile.already_linked";
    public const string CorrectionReasonRequired = "guest_profile.correction_reason_required";
    public const string NoDocumentToCorrect = "guest_profile.no_document_to_correct";
    public const string IdentificationAlreadySet = "guest_profile.identification_already_set";
    public const string GuestProfileInactive = "guest_profile.inactive";

    public const string EmployeeCodeRequired = "staff.employee_code_required";
    public const string EmployeeCodeInvalid = "staff.employee_code_invalid";
    public const string JobPositionRequired = "staff.job_position_required";
    public const string JobPositionLength = "staff.job_position_length";
    public const string StaffProfileInactive = "staff.inactive";
    public const string ShiftInvalid = "staff.shift_invalid";
    public const string HotelNotFound = "staff.hotel_not_found";
    public const string ChainScopeRequiresChainAdmin = "staff.chain_scope_requires_chain_admin";
    public const string ChainAdminNotAllowedAtHotel = "staff.chain_admin_not_allowed_at_hotel";
    public const string ChainAdminAlreadyAssigned = "staff.chain_admin_already_assigned";
    public const string ChainAdminCannotTakeHotelAssignment = "staff.chain_admin_cannot_take_hotel_assignment";
    public const string HotelAssignmentsBlockChainAdmin = "staff.hotel_assignments_block_chain_admin";
    public const string AdminConflictsWithReception = "staff.admin_conflicts_with_reception";
    public const string ReceptionConflictsWithAdmin = "staff.reception_conflicts_with_admin";
    public const string AssignmentDuplicated = "staff.assignment_duplicated";
    public const string AssignmentPeriodExpired = "staff.assignment_period_expired";
    public const string AssignmentTerminated = "staff.assignment_terminated";
    public const string AssignmentNotSuspended = "staff.assignment_not_suspended";
    public const string AssignmentAlreadyTerminated = "staff.assignment_already_terminated";
    public const string TerminationBeforeStart = "staff.termination_before_start";
}
