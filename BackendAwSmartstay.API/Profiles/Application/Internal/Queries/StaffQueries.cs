using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.Queries;

public record GetStaffProfileByIdQuery(StaffProfileId ProfileId);

public record GetStaffProfileByUserIdQuery(UserId UserId);

public record GetStaffProfileByEmployeeCodeQuery(EmployeeCode Code);

public record GetStaffProfilesByHotelIdQuery(TargetId HotelId);

public record GetAllStaffProfilesQuery;
