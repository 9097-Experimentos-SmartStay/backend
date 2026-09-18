using System.ComponentModel.DataAnnotations;
using BackendAwSmartstay.API.Marketing.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Marketing.Interfaces.REST.Validation;

namespace BackendAwSmartstay.API.Marketing.Interfaces.REST.Resources;

/// <summary>
///     The demo form of the landing (US-27). Mirrors its validation; every invalid field is reported in
///     <c>errors</c> under its camelCase name.
/// </summary>
public record CreateDemoRequestResource
{
    /// <summary>First name: 2–50 letters (accents, spaces, hyphens and apostrophes allowed).</summary>
    /// <example>Ana</example>
    [Required, ContactName]
    public string? FirstName { get; init; }

    /// <summary>Last name: 2–50 letters.</summary>
    /// <example>Pérez</example>
    [Required, ContactName]
    public string? LastName { get; init; }

    /// <summary>Name of the hotel or property: 2–100 characters.</summary>
    /// <example>Hotel Casa Andina</example>
    [Required, StringLength(DemoRequest.HotelNameMaxLength, MinimumLength = DemoRequest.HotelNameMinLength)]
    public string? HotelName { get; init; }

    /// <summary>Job title: 2–60 characters.</summary>
    /// <example>Gerente general</example>
    [Required, StringLength(DemoRequest.JobTitleMaxLength, MinimumLength = DemoRequest.JobTitleMinLength)]
    public string? JobTitle { get; init; }

    /// <summary>Contact e-mail (name@domain.tld, max 254). The confirmation is sent here.</summary>
    /// <example>ana.perez@hotel.pe</example>
    [Required, MaxLength(254), ContactEmail]
    public string? Email { get; init; }

    /// <summary><c>boutique</c>, <c>alternative</c> or <c>chain</c>.</summary>
    /// <example>boutique</example>
    [Required, ContractCode("boutique", "alternative", "chain")]
    public string? AccommodationType { get; init; }

    /// <summary><c>1-10</c>, <c>11-30</c>, <c>31-60</c> or <c>60+</c>.</summary>
    /// <example>11-30</example>
    [Required, ContractCode("1-10", "11-30", "31-60", "60+")]
    public string? RoomsRange { get; init; }

    /// <summary><c>search</c>, <c>social</c>, <c>referral</c>, <c>event</c> or <c>other</c>.</summary>
    /// <example>search</example>
    [Required, ContractCode("search", "social", "referral", "event", "other")]
    public string? ReferralSource { get; init; }

    /// <summary>Optional phone: optional <c>+</c> and 7–15 digits (spaces, dots, dashes and parentheses are ignored).</summary>
    /// <example>+51987654321</example>
    [ContactPhone, MaxLength(30)]
    public string? Phone { get; init; }

    /// <summary>Optional message, up to 500 characters.</summary>
    /// <example>Quisiera ver el módulo de reservas.</example>
    [MaxLength(DemoRequest.MessageMaxLength)]
    public string? Message { get; init; }

    /// <summary>Landing profile: <c>admin</c> or <c>guest</c>.</summary>
    /// <example>admin</example>
    [Required, ContractCode("admin", "guest")]
    public string? Profile { get; init; }
}

/// <summary>Immediate confirmation of a demo request (US-27 scenario 1).</summary>
/// <param name="Id">Request id.</param>
/// <param name="Status">Always <c>Received</c>.</param>
/// <param name="Message">Human-readable confirmation.</param>
public record DemoRequestCreatedResource(int Id, string Status, string Message);

/// <summary>A demo request as seen by the sales team.</summary>
public record DemoRequestResource(
    int Id,
    string FirstName,
    string LastName,
    string HotelName,
    string JobTitle,
    string Email,
    string? Phone,
    string AccommodationType,
    string RoomsRange,
    string ReferralSource,
    string Profile,
    string? Message,
    string Status,
    DateTimeOffset ReceivedAt,
    DateTimeOffset? FollowedUpAt);

/// <summary>A page of demo requests.</summary>
public record DemoRequestPageResource(IReadOnlyList<DemoRequestResource> Items, int Page, int PageSize, int TotalCount, int TotalPages);

/// <summary>Result of a follow-up run.</summary>
/// <param name="FollowedUp">Requests that got their reminder in this run (0 when nothing was due).</param>
public record DemoFollowUpResultResource(int FollowedUp);
