using System.ComponentModel.DataAnnotations;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Validation;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>A refresh token presented to renew or end a remembered session.</summary>
public record RefreshTokenResource
{
    /// <summary>The refresh token returned by sign-in (with <c>rememberMe</c>) or by the last refresh.</summary>
    [Required]
    [MaxLength(200)]
    public string? RefreshToken { get; init; }
}

/// <summary>The token of a link received by e-mail.</summary>
public record VerifyEmailResource
{
    /// <summary>Value of the <c>token</c> query parameter of the verification link.</summary>
    [Required]
    [MaxLength(200)]
    public string? Token { get; init; }
}

/// <summary>An e-mail address (resend verification, password recovery).</summary>
public record AccountEmailResource
{
    /// <summary>The account e-mail.</summary>
    /// <example>ana.perez@example.com</example>
    [Required]
    [AccountEmail]
    [MaxLength(254)]
    public string? Email { get; init; }
}

/// <summary>New password chosen through the recovery link (US-04).</summary>
public record ResetPasswordResource
{
    /// <summary>Value of the <c>token</c> query parameter of the reset link.</summary>
    [Required]
    [MaxLength(200)]
    public string? Token { get; init; }

    /// <summary>The new password, 8 to 128 characters.</summary>
    /// <example>NuevaClave#2026</example>
    [Required]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "The password must have between 8 and 128 characters.")]
    public string? NewPassword { get; init; }
}

/// <summary>A newly registered account (US-01).</summary>
/// <param name="Id">The new user id.</param>
/// <param name="Email">The account e-mail.</param>
/// <param name="EmailVerified">Always false: a verification link was sent.</param>
/// <param name="Message">Human-readable confirmation.</param>
public record SignUpResultResource(int Id, string Email, bool EmailVerified, string Message);

/// <summary>A human-readable confirmation.</summary>
/// <param name="Message">The message.</param>
public record MessageResource(string Message);
