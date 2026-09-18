using BackendAwSmartstay.Domain.Profiles.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

public record DateRange
{
    public DateOnly StartDate { get; }
    public DateOnly? EndDate { get; }

    public DateRange(DateOnly startDate, DateOnly? endDate = null)
    {
        if (endDate.HasValue && endDate.Value < startDate)
            throw new DomainValidationException(ProfileErrorCodes.DateRangeInvalid, "EndDate cannot be earlier than StartDate.");

        StartDate = startDate;
        EndDate = endDate;
    }

    public bool Includes(DateOnly date) =>
        date >= StartDate && (!EndDate.HasValue || date <= EndDate.Value);
}
